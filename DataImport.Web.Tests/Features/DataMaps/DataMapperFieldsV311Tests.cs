// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Threading.Tasks;
using DataImport.TestHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using NUnit.Framework;
using static DataImport.Web.Tests.Testing;

namespace DataImport.Web.Tests.Features.DataMaps
{
    [TestFixture]
    public class DataMapperFieldsV311Tests : DataMapperFieldsTestBase
    {
        public override async Task SetUpResources()
        {
            await ConfigureForOdsApiV311();
        }

        [Test]
        public override async Task ShouldGetUserFacingSourceColumnSelectionListGivenRepresentativeCsvHeaders()
        {
            var undefinedCsv = await GetDataMapperFields("/ed-fi/students");

            undefinedCsv.SourceColumns.ShouldMatch(_emptySourceColumns);

            var definedCsv = await GetDataMapperFields("/ed-fi/students", new[] { "Header1", "Header2", "Header3" });

            definedCsv.SourceColumns.ShouldMatch(
                new SelectListItem { Text = "Select Source Column", Value = "" },
                new SelectListItem { Text = "Header1", Value = "Header1" },
                new SelectListItem { Text = "Header2", Value = "Header2" },
                new SelectListItem { Text = "Header3", Value = "Header3" });

            var complexCsv = await GetDataMapperFields("/ed-fi/students", new[] { "Header 1", "Header 2", "Header 3 contains a comma, and extra whitespace" });

            complexCsv.SourceColumns.ShouldMatch(
                new SelectListItem { Text = "Select Source Column", Value = "" },
                new SelectListItem { Text = "Header 1", Value = "Header 1" },
                new SelectListItem { Text = "Header 2", Value = "Header 2" },
                new SelectListItem { Text = "Header 3 contains a comma, and extra whitespace", Value = "Header 3 contains a comma, and extra whitespace" });
        }

        [Test]
        public override async Task ShouldGetUserFacingMappableFieldsForStudentsResource()
        {
            var response = await GetDataMapperFields("/ed-fi/students");

            response.ResourceMetadata.ShouldMatch(
                RequiredFieldMetadata("studentUniqueId", "string"),
                FieldMetadata("birthCity", "string"),
                FieldMetadata("birthCountryDescriptor", "string"),
                RequiredFieldMetadata("birthDate", "string"),
                FieldMetadata("birthInternationalProvince", "string"),
                FieldMetadata("birthSexDescriptor", "string"),
                FieldMetadata("birthStateAbbreviationDescriptor", "string"),
                FieldMetadata("citizenshipStatusDescriptor", "string"),
                FieldMetadata("dateEnteredUS", "string"),
                RequiredFieldMetadata("firstName", "string"),
                FieldMetadata("generationCodeSuffix", "string"),

                FieldMetadata("identificationDocuments", "array",
                    FieldMetadata("studentIdentificationDocument", "edFi_studentIdentificationDocument",
                        RequiredFieldMetadata("identificationDocumentUseDescriptor", "string"),
                        RequiredFieldMetadata("personalInformationVerificationDescriptor", "string"),
                        FieldMetadata("issuerCountryDescriptor", "string"),
                        FieldMetadata("documentExpirationDate", "string"),
                        FieldMetadata("documentTitle", "string"),
                        FieldMetadata("issuerDocumentIdentificationCode", "string"),
                        FieldMetadata("issuerName", "string"))),

                RequiredFieldMetadata("lastSurname", "string"),
                FieldMetadata("maidenName", "string"),
                FieldMetadata("middleName", "string"),
                FieldMetadata("multipleBirthStatus", "boolean"),

                FieldMetadata("otherNames", "array",
                    FieldMetadata("studentOtherName", "edFi_studentOtherName",
                        RequiredFieldMetadata("otherNameTypeDescriptor", "string"),
                        RequiredFieldMetadata("firstName", "string"),
                        FieldMetadata("generationCodeSuffix", "string"),
                        RequiredFieldMetadata("lastSurname", "string"),
                        FieldMetadata("middleName", "string"),
                        FieldMetadata("personalTitlePrefix", "string"))),

                FieldMetadata("personalIdentificationDocuments", "array",
                    FieldMetadata("studentPersonalIdentificationDocument", "edFi_studentPersonalIdentificationDocument",
                        RequiredFieldMetadata("identificationDocumentUseDescriptor", "string"),
                        RequiredFieldMetadata("personalInformationVerificationDescriptor", "string"),
                        FieldMetadata("issuerCountryDescriptor", "string"),
                        FieldMetadata("documentExpirationDate", "string"),
                        FieldMetadata("documentTitle", "string"),
                        FieldMetadata("issuerDocumentIdentificationCode", "string"),
                        FieldMetadata("issuerName", "string"))),

                FieldMetadata("personalTitlePrefix", "string"),

                FieldMetadata("visas", "array",
                    FieldMetadata("studentVisa", "edFi_studentVisa",
                        RequiredFieldMetadata("visaDescriptor", "string"))));

            response.Mappings.ShouldMatch(
                Field("studentUniqueId"),
                Field("birthCity"),
                Field("birthCountryDescriptor"),
                Field("birthDate"),
                Field("birthInternationalProvince"),
                Field("birthSexDescriptor"),
                Field("birthStateAbbreviationDescriptor"),
                Field("citizenshipStatusDescriptor"),
                Field("dateEnteredUS"),
                Field("firstName"),
                Field("generationCodeSuffix"),
                Field("identificationDocuments"),
                Field("lastSurname"),
                Field("maidenName"),
                Field("middleName"),
                Field("multipleBirthStatus"),
                Field("otherNames"),
                Field("personalIdentificationDocuments"),
                Field("personalTitlePrefix"),
                Field("visas"));
        }

        [Test]
        public override async Task ShouldGetUserFacingMappableFieldsForStudentAssessmentsResource()
        {
            var response = await GetDataMapperFields("/ed-fi/studentAssessments");

            response.ResourceMetadata.ShouldMatch(

                RequiredFieldMetadata("studentAssessmentIdentifier", "string"),

                RequiredFieldMetadata("assessmentReference", "edFi_assessmentReference",
                    RequiredFieldMetadata("assessmentIdentifier", "string"),
                    RequiredFieldMetadata("namespace", "string")),

                FieldMetadata("schoolYearTypeReference", "edFi_schoolYearTypeReference",
                    RequiredFieldMetadata("schoolYear", "integer")),

                RequiredFieldMetadata("studentReference", "edFi_studentReference",
                    RequiredFieldMetadata("studentUniqueId", "string")),

                FieldMetadata("accommodations", "array",
                    FieldMetadata("studentAssessmentAccommodation", "edFi_studentAssessmentAccommodation",
                        RequiredFieldMetadata("accommodationDescriptor", "string"))),

                RequiredFieldMetadata("administrationDate", "string"),
                FieldMetadata("administrationEndDate", "string"),
                FieldMetadata("administrationEnvironmentDescriptor", "string"),
                FieldMetadata("administrationLanguageDescriptor", "string"),
                FieldMetadata("eventCircumstanceDescriptor", "string"),
                FieldMetadata("eventDescription", "string"),

                FieldMetadata("items", "array",
                    FieldMetadata("studentAssessmentItem", "edFi_studentAssessmentItem",
                        RequiredFieldMetadata("assessmentItemResultDescriptor", "string"),
                        FieldMetadata("responseIndicatorDescriptor", "string"),
                        FieldMetadata("assessmentResponse", "string"),
                        FieldMetadata("descriptiveFeedback", "string"),
                        FieldMetadata("rawScoreResult", "integer"),
                        FieldMetadata("timeAssessed", "string"),
                        RequiredFieldMetadata("assessmentItemReference", "edFi_assessmentItemReference",
                            RequiredFieldMetadata("assessmentIdentifier", "string"),
                            RequiredFieldMetadata("identificationCode", "string"),
                            RequiredFieldMetadata("namespace", "string")))),

                FieldMetadata("performanceLevels", "array",
                    FieldMetadata("studentAssessmentPerformanceLevel", "edFi_studentAssessmentPerformanceLevel",
                        RequiredFieldMetadata("assessmentReportingMethodDescriptor", "string"),
                        RequiredFieldMetadata("performanceLevelDescriptor", "string"),
                        RequiredFieldMetadata("performanceLevelMet", "boolean"))),

                FieldMetadata("reasonNotTestedDescriptor", "string"),
                FieldMetadata("retestIndicatorDescriptor", "string"),

                FieldMetadata("scoreResults", "array",
                    FieldMetadata("studentAssessmentScoreResult", "edFi_studentAssessmentScoreResult",
                        RequiredFieldMetadata("assessmentReportingMethodDescriptor", "string"),
                        RequiredFieldMetadata("resultDatatypeTypeDescriptor", "string"),
                        RequiredFieldMetadata("result", "string"))),

                FieldMetadata("serialNumber", "string"),

                FieldMetadata("studentObjectiveAssessments", "array",
                    FieldMetadata("studentAssessmentStudentObjectiveAssessment", "edFi_studentAssessmentStudentObjectiveAssessment",
                        RequiredFieldMetadata("objectiveAssessmentReference", "edFi_objectiveAssessmentReference",
                            RequiredFieldMetadata("assessmentIdentifier", "string"),
                            RequiredFieldMetadata("identificationCode", "string"),
                            RequiredFieldMetadata("namespace", "string")),
                        FieldMetadata("performanceLevels", "array",
                            FieldMetadata("studentAssessmentStudentObjectiveAssessmentPerformanceLevel", "edFi_studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                RequiredFieldMetadata("assessmentReportingMethodDescriptor", "string"),
                                RequiredFieldMetadata("performanceLevelDescriptor", "string"),
                                RequiredFieldMetadata("performanceLevelMet", "boolean"))),
                        RequiredFieldMetadata("scoreResults", "array",
                            FieldMetadata("studentAssessmentStudentObjectiveAssessmentScoreResult", "edFi_studentAssessmentStudentObjectiveAssessmentScoreResult",
                                RequiredFieldMetadata("assessmentReportingMethodDescriptor", "string"),
                                RequiredFieldMetadata("resultDatatypeTypeDescriptor", "string"),
                                RequiredFieldMetadata("result", "string"))))),

                FieldMetadata("whenAssessedGradeLevelDescriptor", "string")
            );

            response.Mappings.ShouldMatch(

                Field("studentAssessmentIdentifier"),

                Field("assessmentReference",
                    Field("assessmentIdentifier"),
                    Field("namespace")),

                Field("schoolYearTypeReference",
                    Field("schoolYear")),

                Field("studentReference",
                    Field("studentUniqueId")),

                // Array mappings begin with zero items.
                Field("accommodations"),
                Field("administrationDate"),
                Field("administrationEndDate"),
                Field("administrationEnvironmentDescriptor"),
                Field("administrationLanguageDescriptor"),
                Field("eventCircumstanceDescriptor"),
                Field("eventDescription"),
                Field("items"),
                Field("performanceLevels"),
                Field("reasonNotTestedDescriptor"),
                Field("retestIndicatorDescriptor"),
                Field("scoreResults"),
                Field("serialNumber"),
                Field("studentObjectiveAssessments"),
                Field("whenAssessedGradeLevelDescriptor")
            );
        }
    }
}
