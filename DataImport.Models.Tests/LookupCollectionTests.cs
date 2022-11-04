// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Linq;
using NUnit.Framework;
using Shouldly;

namespace DataImport.Models.Tests
{
    public class LookupCollectionTests
    {
        private LookupCollection lookups;

        [SetUp]
        public void Init()
        {
            lookups = new LookupCollection(new[]
            {
                Lookup("Table A", "Key A 1", "Value A 1"),
                Lookup(" Table A ", " Key A 2 ", " Value A 2 "),

                Lookup("Table B", "Key B 1", "Value B 1"),
                Lookup(" Table B ", " Key B 2 ", " Value B 2 "),
                Lookup("  Table B  ", "  Key B 3  ", "  Value B 3  ")
            });
        }

        [Test]
        public void ShouldLookUpValuesBySourceTableAndKeyWithoutThrowingExceptions()
        {
            string valueA1;
            lookups.TryLookup("Table A", "Key A 1", out valueA1).ShouldBeTrue();
            valueA1.ShouldBe("Value A 1");

            string valueA2;
            lookups.TryLookup("Table A", "Key A 2", out valueA2).ShouldBeTrue();
            valueA2.ShouldBe("Value A 2");

            string valueB1;
            lookups.TryLookup("Table B", "Key B 1", out valueB1).ShouldBeTrue();
            valueB1.ShouldBe("Value B 1");

            string valueB2;
            lookups.TryLookup("Table B", "Key B 2", out valueB2).ShouldBeTrue();
            valueB2.ShouldBe("Value B 2");

            string valueB3;
            lookups.TryLookup("Table B", "Key B 3", out valueB3).ShouldBeTrue();
            valueB3.ShouldBe("Value B 3");

            string noMatchBySourceTable;
            lookups.TryLookup("Unexpected Table", "Key A 1", out noMatchBySourceTable).ShouldBeFalse();
            noMatchBySourceTable.ShouldBeNull();

            string noMatchByKey;
            lookups.TryLookup("Table A", "Unexpected Key A 1", out noMatchByKey).ShouldBeFalse();
            noMatchByKey.ShouldBeNull();
        }

        [Test]
        public void ShouldGiveExpectedSourceTablesList()
        {
            lookups.SourceTables().ShouldNotBeNull();
            lookups.SourceTables().ShouldNotBeEmpty();
            lookups.SourceTables().Count().ShouldBe(2);
            lookups.SourceTables().ShouldContain("Table A");
            lookups.SourceTables().ShouldContain("Table B");
        }
    
        private int _id;
        private Lookup Lookup(string sourceTable, string key, string value)
        {
            _id++;
            return new Lookup
            {
                Id = _id,
                SourceTable = sourceTable,
                Key = key,
                Value = value
            };
        }
    }
}
