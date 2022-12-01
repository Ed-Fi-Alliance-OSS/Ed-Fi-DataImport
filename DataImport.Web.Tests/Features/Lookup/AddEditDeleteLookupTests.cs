// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using DataImport.Models;
using DataImport.TestHelpers;
using Shouldly;
using DataImport.Web.Features.Lookup;
using NUnit.Framework;
using static DataImport.Web.Tests.Testing;
using System.Threading.Tasks;

namespace DataImport.Web.Tests.Features.Lookup
{
    internal class AddEditDeleteLookupTests
    {
        private int _notReferencedLookUpIdC;
        private int _notReferencedLookUpIdD;
        private int _referencedLookUpIdA;
        private int _referencedLookUpIdB;
        private int _referencedLookUpIdC;
        private int _referencedWithSingleLookUpId;
        private int _referencedSourceTableWithSingleLookUpEditId;
        private string _referencedSourceTable;
        private string _referencedSourceTableWithSingleLookUp;
        private string _referencedSourceTableWithSingleLookUpEdit;
        private string _notReferencedSourceTable;
        private string _sourceTable;
        private int _apiVersionId;

        [SetUp]
        public async Task Init()
        {
            _apiVersionId = Query(d => d.ApiVersions.First()).Id;

            _referencedSourceTable = SampleString("ReferencedSourceTable");
            _notReferencedSourceTable = SampleString("NotReferencedSourceTable");

            _referencedSourceTableWithSingleLookUp = SampleString("WithSingleLookUp");
            _referencedSourceTableWithSingleLookUpEdit = SampleString("EditWithSingleLookUp");

            var usableResources = Query(x => x.Resources.Where(y => y.ApiVersionId == _apiVersionId).ToArray())
                .Where(x =>
                {
                    var metadata = ResourceMetadata.DeserializeFrom(x);

                    var hasTwoOrMoreTopLevelNonArrayProperties =
                        metadata.Count(m => m.DataType != "array") >= 2;

                    return hasTwoOrMoreTopLevelNonArrayProperties;
                })
                .ToArray();

            var resource = RandomItem(usableResources);
            var resource1 = RandomItem(usableResources);
            var resource2 = RandomItem(usableResources);

            _referencedLookUpIdA = (await AddLookup(_referencedSourceTable, "Key A", "Value A")).Id;
            _referencedLookUpIdB = (await AddLookup(_referencedSourceTable, "Key B", "Value B")).Id;
            _referencedLookUpIdC = (await AddLookup(_referencedSourceTable, "Key C", "Value C")).Id;

            _notReferencedLookUpIdC = (await AddLookup(_notReferencedSourceTable, "Key D", "Value D")).Id;
            _notReferencedLookUpIdD = (await AddLookup(_notReferencedSourceTable, "Key E", "Value E")).Id;

            _referencedWithSingleLookUpId = (await AddLookup(_referencedSourceTableWithSingleLookUp, "Key F", "Value F")).Id;
            _referencedSourceTableWithSingleLookUpEditId = (await AddLookup(_referencedSourceTableWithSingleLookUpEdit, "Key G", "Value G")).Id;

            await CreateDataMap(resource, _referencedSourceTable);
            await CreateDataMap(resource1, _referencedSourceTableWithSingleLookUp);
            await CreateDataMap(resource2, _referencedSourceTableWithSingleLookUpEdit);
        }

        private async Task CreateDataMap(Resource resource, string sourceTable)
        {
            _sourceTable = sourceTable;
            var mappings = await TrivialMappings(resource);
            SelectLookupTables(mappings, ResourceMetadata.DeserializeFrom(resource));
            var columnHeaders = new[] { "Csv Column A", "Csv Column B", "Csv Column C" };
            await AddDataMap(resource, columnHeaders, mappings);
        }

        [Test]
        public void ShouldRequireMinimumFields()
        {
            new AddLookup.Command()
                .ShouldNotValidate(
                    "'Source Table' must not be empty.",
                    "'Key' must not be empty.",
                    "'Value' must not be empty.");

            new EditLookup.Command()
                .ShouldNotValidate(
                    "'Source Table' must not be empty.",
                    "'Key' must not be empty.",
                    "'Value' must not be empty.");
        }

        [Test]
        public void ShouldRequireFieldsWithAppropriateLengths()
        {
            new AddLookup.Command
            {
                SourceTable = SampleString(1025),
                Key = SampleString(1026),
                Value = SampleString(1027)
            }.ShouldNotValidate(
                "The length of \'Source Table\' must be 1024 characters or fewer. You entered 1025 characters.",
                "The length of \'Key\' must be 1024 characters or fewer. You entered 1026 characters.",
                "The length of \'Value\' must be 1024 characters or fewer. You entered 1027 characters.");

            new EditLookup.Command
            {
                SourceTable = SampleString(1025),
                Key = SampleString(1026),
                Value = SampleString(1027)
            }.ShouldNotValidate(
                "The length of \'Source Table\' must be 1024 characters or fewer. You entered 1025 characters.",
                "The length of \'Key\' must be 1024 characters or fewer. You entered 1026 characters.",
                "The length of \'Value\' must be 1024 characters or fewer. You entered 1027 characters.");
        }

        [Test]
        public async Task ShouldSuccessfullyAddTrimmedLookup()
        {
            var sourceTable = SampleString("SourceTable");
            var key = SampleString("Key");
            var value = SampleString("Value");

            var addForm = new AddLookup.Command
            {
                SourceTable = " " + sourceTable + " ",
                Key = " " + key + " ",
                Value = " " + value + " "
            };

            var response = await Send(addForm);
            response.AssertToast($"Lookup '{key}' was created.");

            var lookupId = response.LookupId;

            var editForm = await Send(new EditLookup.Query { Id = lookupId });

            editForm.ShouldMatch(new EditLookup.Command
            {
                Id = lookupId,
                SourceTable = sourceTable,
                Key = key,
                Value = value
            });
        }

        [Test]
        public async Task ShouldKeyBeUniqueOnTheSourceTableWhileAddingLookUp()
        {
            var sourceTable = SampleString("SourceTable");
            var key = SampleString("Key");
            var value = SampleString("Value");

            var existingLookUp = new AddLookup.Command
            {
                SourceTable = sourceTable,
                Key = key,
                Value = value
            };

            await Send(existingLookUp);

            new AddLookup.Command
            {
                SourceTable = sourceTable,
                Key = key,
                Value = "differentValue"
            }.ShouldNotValidate($"Lookup key '{key}' already exists on the source table '{sourceTable}'. Please try different key.");
        }

        [Test]
        public async Task ShouldKeyBeUniqueOnTheSourceTableWhileEditingLookUp()
        {
            var sourceTable = SampleString("SourceTable");
            var key = SampleString("Key");
            var value = SampleString("Value");

            var existingLookUp = new AddLookup.Command
            {
                SourceTable = sourceTable,
                Key = key,
                Value = value
            };

            await Send(existingLookUp);

            new EditLookup.Command
            {
                SourceTable = sourceTable,
                Key = key,
                Value = value
            }.ShouldNotValidate($"Lookup key '{key}' already exists on the source table '{sourceTable}'. Please try different key.");
        }

        [Test]
        public async Task ShouldSaveWhileSavingWithoutChangingAnythingOnEditLookUp()
        {
            var sourceTable = SampleString("SourceTable");
            var key = SampleString("Key");
            var value = SampleString("Value");

            var existingLookUp = new AddLookup.Command
            {
                SourceTable = sourceTable,
                Key = key,
                Value = value
            };

            var lookupId = (await Send(existingLookUp)).LookupId;

            new EditLookup.Command
            {
                Id = lookupId,
                SourceTable = sourceTable,
                Key = key,
                Value = value
            }.ShouldValidate();
        }

        [Test]
        public async Task ShouldSuccessfullyEditTrimmedLookup()
        {
            var addForm = new AddLookup.Command
            {
                SourceTable = SampleString("SourceTable"),
                Key = SampleString("Key"),
                Value = SampleString("Value")
            };

            var lookupId = (await Send(addForm)).LookupId;

            var editForm = await Send(new EditLookup.Query { Id = lookupId });

            var newSourceTable = SampleString("New SourceTable");
            var newKey = SampleString("New Key");
            var newValue = SampleString("New Value");

            editForm.SourceTable = " " + newSourceTable + " ";
            editForm.Key = " " + newKey + " ";
            editForm.Value = " " + newValue + " ";

            var response = await Send(editForm);
            response.AssertToast($"Lookup '{newKey}' was modified.");

            var updatedEditForm = await Send(new EditLookup.Query { Id = lookupId });

            updatedEditForm.ShouldMatch(new EditLookup.Command
            {
                Id = lookupId,
                SourceTable = newSourceTable,
                Key = newKey,
                Value = newValue
            });
        }

        [Test]
        public async Task ShouldSuccessfullyEditNotReferencedLookupSourceTableName()
        {
            var editForm = await Send(new EditLookup.Query { Id = _notReferencedLookUpIdC });

            var newSourceTable = SampleString("New SourceTable");
            var newKey = SampleString("New Key");
            var newValue = SampleString("New Value");

            editForm.SourceTable = " " + newSourceTable + " ";
            editForm.Key = " " + newKey + " ";
            editForm.Value = " " + newValue + " ";

            await Send(editForm);

            var updatedEditForm = await Send(new EditLookup.Query { Id = _notReferencedLookUpIdC });

            updatedEditForm.ShouldMatch(new EditLookup.Command
            {
                Id = _notReferencedLookUpIdC,
                SourceTable = newSourceTable,
                Key = newKey,
                Value = newValue
            });
        }

        [Test]
        public async Task ShouldSuccessfullyEditReferencedAndSameLookupSourceTableName()
        {
            var editForm = await Send(new EditLookup.Query { Id = _referencedLookUpIdA });

            var sourceTable = _referencedSourceTable;
            var newKey = SampleString("New Key");
            var newValue = SampleString("New Value");

            editForm.SourceTable = " " + sourceTable + " ";
            editForm.Key = " " + newKey + " ";
            editForm.Value = " " + newValue + " ";

            await Send(editForm);

            var updatedEditForm = await Send(new EditLookup.Query { Id = _referencedLookUpIdA });

            updatedEditForm.ShouldMatch(new EditLookup.Command
            {
                Id = _referencedLookUpIdA,
                SourceTable = sourceTable,
                Key = newKey,
                Value = newValue
            });
        }

        [Test]
        public async Task ShouldSuccessfullyEditReferencedAndWithMoreThanOneLookUps()
        {
            var editForm = await Send(new EditLookup.Query { Id = _referencedLookUpIdB });

            var sourceTable = SampleString("New SourceTable");
            var newKey = SampleString("New Key");
            var newValue = SampleString("New Value");

            editForm.SourceTable = " " + sourceTable + " ";
            editForm.Key = " " + newKey + " ";
            editForm.Value = " " + newValue + " ";

            await Send(editForm);

            var updatedEditForm = await Send(new EditLookup.Query { Id = _referencedLookUpIdB });

            updatedEditForm.ShouldMatch(new EditLookup.Command
            {
                Id = _referencedLookUpIdB,
                SourceTable = sourceTable,
                Key = newKey,
                Value = newValue
            });
        }

        [Test]
        public async Task ShouldGetValidationErrorOnEditingReferencedLookupSourceTableName()
        {
            var editForm = await Send(new EditLookup.Query { Id = _referencedSourceTableWithSingleLookUpEditId });

            var newSourceTable = SampleString("New SourceTable");
            var newKey = SampleString("New Key");
            var newValue = SampleString("New Value");

            editForm.SourceTable = " " + newSourceTable + " ";
            editForm.Key = " " + newKey + " ";
            editForm.Value = " " + newValue + " ";
            editForm.ShouldNotValidate("The source table name cannot be edited because it is referenced by another data map. Please remove all references first before editing the lookup's source table name.");
        }

        [Test]
        public async Task ShouldSuccessfullyDeleteNotReferencedLookupSourceTable()
        {
            var lookupKey = Query<DataImport.Models.Lookup>(_notReferencedLookUpIdD).Key;
            var response = await Send(new DeleteLookup.Command
            {
                Id = _notReferencedLookUpIdD
            });
            response.AssertToast($"Lookup '{lookupKey}' was deleted.");

            Query<DataImport.Models.Lookup>(_notReferencedLookUpIdD).ShouldBeNull();
        }

        [Test]
        public async Task ShouldSuccessfullyDeleteReferencedWithMoreThanOneLookUps()
        {
            await Send(new DeleteLookup.Command
            {
                Id = _referencedLookUpIdC
            });
            Query<DataImport.Models.Lookup>(_referencedLookUpIdC).ShouldBeNull();
        }

        [Test]
        public void ShouldGetValidationErrorOnDeletingReferencedLookUpTableWithSingleLookUp()
        {
            new DeleteLookup.Command
            {
                Id = _referencedWithSingleLookUpId
            }.ShouldNotValidate("The lookup cannot be deleted because it is referenced by another data map. Please remove all references before deleting the lookup.");
        }

        private void SelectLookupTables(IReadOnlyList<DataMapper> mappings, IReadOnlyList<ResourceMetadata> metaDataList)
        {
            foreach (var mapping in mappings)
            {
                var metadata = metaDataList.Single(m => m.Name == mapping.Name);

                if (metadata.DataType == "array")
                    continue;

                if (mapping.Children.Any())
                {
                    SelectLookupTables(mapping.Children, metadata.Children);
                }
                else
                {
                    mapping.SourceColumn = "Csv Column A";
                    mapping.SourceTable = _sourceTable;
                }
            }
        }
    }
}
