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
    public class DataMapperFieldsV25Tests : DataMapperFieldsTestBase
    {
        public override async Task SetUpResources()
        {
            await ConfigureForOdsApiV25();
        }

        [Test]
        public override async Task ShouldGetUserFacingSourceColumnSelectionListGivenRepresentativeCsvHeaders()
        {
            var undefinedCsv = await GetDataMapperFields("/students");

            undefinedCsv.SourceColumns.ShouldMatch(_emptySourceColumns);

            var definedCsv = await GetDataMapperFields("/students", new []{"Header1", "Header2", "Header3"});

            definedCsv.SourceColumns.ShouldMatch(
                new SelectListItem { Text = "Select Source Column", Value = "" },
                new SelectListItem { Text = "Header1", Value = "Header1" },
                new SelectListItem { Text = "Header2", Value = "Header2" },
                new SelectListItem { Text = "Header3", Value = "Header3" });

            var complexCsv = await GetDataMapperFields("/students", new []{"Header 1", "Header 2", "Header 3 contains a comma, and extra whitespace"});

            complexCsv.SourceColumns.ShouldMatch(
                new SelectListItem { Text = "Select Source Column", Value = "" },
                new SelectListItem { Text = "Header 1", Value = "Header 1" },
                new SelectListItem { Text = "Header 2", Value = "Header 2" },
                new SelectListItem { Text = "Header 3 contains a comma, and extra whitespace", Value = "Header 3 contains a comma, and extra whitespace" });
        }

        [Test]
        public override async Task ShouldGetUserFacingMappableFieldsForStudentsResource()
        {
            var response = await GetDataMapperFields("/students");

            response.ResourceMetadata.ShouldMatch(
                FieldMetadata("birthCity", "string"),
                FieldMetadata("birthCountryDescriptor", "string"),
                RequiredFieldMetadata("birthDate", "string"),
                FieldMetadata("birthInternationalProvince", "string"),
                FieldMetadata("birthStateAbbreviationType", "string"),
                FieldMetadata("citizenshipStatusType", "string"),
                FieldMetadata("dateEnteredUS", "string"),
                FieldMetadata("displacementStatus", "string"),
                FieldMetadata("economicDisadvantaged", "boolean"),
                RequiredFieldMetadata("firstName", "string"),
                FieldMetadata("generationCodeSuffix", "string"),
                RequiredFieldMetadata("hispanicLatinoEthnicity", "boolean"),
                RequiredFieldMetadata("lastSurname", "string"),
                FieldMetadata("limitedEnglishProficiencyDescriptor", "string"),
                FieldMetadata("loginId", "string"),
                FieldMetadata("maidenName", "string"),
                FieldMetadata("middleName", "string"),
                FieldMetadata("multipleBirthStatus", "boolean"),
                FieldMetadata("oldEthnicityType", "string"),
                FieldMetadata("personalTitlePrefix", "string"),
                FieldMetadata("profileThumbnail", "string"),
                FieldMetadata("schoolFoodServicesEligibilityDescriptor", "string"),
                RequiredFieldMetadata("sexType", "string"),
                RequiredFieldMetadata("studentUniqueId", "string"),

                FieldMetadata("learningStyle", "studentLearningStyle",
                    RequiredFieldMetadata("visualLearning", "number"),
                    RequiredFieldMetadata("auditoryLearning", "number"),
                    RequiredFieldMetadata("tactileLearning", "number")),

                FieldMetadata("addresses", "array",
                    FieldMetadata("studentAddress", "studentAddress",
                        RequiredFieldMetadata("addressType", "string"),
                        RequiredFieldMetadata("stateAbbreviationType", "string"),
                        RequiredFieldMetadata("streetNumberName", "string"),
                        FieldMetadata("apartmentRoomSuiteNumber", "string"),
                        FieldMetadata("buildingSiteNumber", "string"),
                        RequiredFieldMetadata("city", "string"),
                        RequiredFieldMetadata("postalCode", "string"),
                        FieldMetadata("nameOfCounty", "string"),
                        FieldMetadata("countyFIPSCode", "string"),
                        FieldMetadata("latitude", "string"),
                        FieldMetadata("longitude", "string"),
                        FieldMetadata("beginDate", "string"),
                        FieldMetadata("endDate", "string"))),

                FieldMetadata("characteristics", "array",
                    FieldMetadata("studentCharacteristic", "studentCharacteristic",
                        RequiredFieldMetadata("descriptor", "string"),
                        FieldMetadata("beginDate", "string"),
                        FieldMetadata("endDate", "string"),
                        FieldMetadata("designatedBy", "string"))),

                FieldMetadata("cohortYears", "array",
                    FieldMetadata("studentCohortYear", "studentCohortYear",
                        RequiredFieldMetadata("schoolYearTypeReference", "schoolYearTypeReference",
                            RequiredFieldMetadata("schoolYear", "integer")),
                        RequiredFieldMetadata("cohortYearType", "string"))),

                FieldMetadata("disabilities", "array",
                    FieldMetadata("studentDisability", "studentDisability",
                        RequiredFieldMetadata("disabilityDescriptor", "string"),
                        FieldMetadata("disabilityDeterminationSourceType", "string"),
                        FieldMetadata("disabilityDiagnosis", "string"),
                        FieldMetadata("orderOfDisability", "integer"))),

                FieldMetadata("electronicMails", "array",
                    FieldMetadata("studentElectronicMail", "studentElectronicMail",
                        RequiredFieldMetadata("electronicMailType", "string"),
                        RequiredFieldMetadata("electronicMailAddress", "string"),
                        FieldMetadata("primaryEmailAddressIndicator", "boolean"))),

                FieldMetadata("identificationCodes", "array",
                    FieldMetadata("studentIdentificationCode", "studentIdentificationCode",
                        RequiredFieldMetadata("studentIdentificationSystemDescriptor", "string"),
                        RequiredFieldMetadata("assigningOrganizationIdentificationCode", "string"),
                        RequiredFieldMetadata("identificationCode", "string"))),

                FieldMetadata("identificationDocuments", "array",
                    FieldMetadata("studentIdentificationDocument", "studentIdentificationDocument",
                        RequiredFieldMetadata("identificationDocumentUseType", "string"),
                        RequiredFieldMetadata("personalInformationVerificationType", "string"),
                        FieldMetadata("issuerCountryDescriptor", "string"),
                        FieldMetadata("documentTitle", "string"),
                        FieldMetadata("documentExpirationDate", "string"),
                        FieldMetadata("issuerDocumentIdentificationCode", "string"),
                        FieldMetadata("issuerName", "string"))),

                FieldMetadata("indicators", "array",
                    FieldMetadata("studentIndicator", "studentIndicator",
                        RequiredFieldMetadata("indicatorName", "string"),
                        FieldMetadata("indicatorGroup", "string"),
                        RequiredFieldMetadata("indicator", "string"),
                        FieldMetadata("beginDate", "string"),
                        FieldMetadata("endDate", "string"),
                        FieldMetadata("designatedBy", "string"))),

                FieldMetadata("internationalAddresses", "array",
                    FieldMetadata("studentInternationalAddress", "studentInternationalAddress",
                        RequiredFieldMetadata("addressType", "string"),
                        RequiredFieldMetadata("countryDescriptor", "string"),
                        RequiredFieldMetadata("addressLine1", "string"),
                        FieldMetadata("addressLine2", "string"),
                        FieldMetadata("addressLine3", "string"),
                        FieldMetadata("addressLine4", "string"),
                        FieldMetadata("latitude", "string"),
                        FieldMetadata("longitude", "string"),
                        FieldMetadata("beginDate", "string"),
                        FieldMetadata("endDate", "string"))),

                FieldMetadata("languages", "array",
                    FieldMetadata("studentLanguage", "studentLanguage",
                        RequiredFieldMetadata("languageDescriptor", "string"),
                        FieldMetadata("uses", "array",
                            FieldMetadata("studentLanguageUse", "studentLanguageUse",
                                RequiredFieldMetadata("languageUseType", "string"))))),

                FieldMetadata("otherNames", "array",
                    FieldMetadata("studentOtherName", "studentOtherName",
                        RequiredFieldMetadata("otherNameType", "string"),
                        FieldMetadata("personalTitlePrefix", "string"),
                        RequiredFieldMetadata("firstName", "string"),
                        FieldMetadata("middleName", "string"),
                        RequiredFieldMetadata("lastSurname", "string"),
                        FieldMetadata("generationCodeSuffix", "string"))),

                FieldMetadata("programParticipations", "array",
                    FieldMetadata("studentProgramParticipation", "studentProgramParticipation",
                        RequiredFieldMetadata("programType", "string"),
                        FieldMetadata("beginDate", "string"),
                        FieldMetadata("endDate", "string"),
                        FieldMetadata("designatedBy", "string"),
                        FieldMetadata("programCharacteristics", "array",
                            FieldMetadata("studentProgramParticipationProgramCharacteristic",
                                "studentProgramParticipationProgramCharacteristic",
                                RequiredFieldMetadata("programCharacteristicDescriptor", "string"))))),

                FieldMetadata("races", "array",
                    FieldMetadata("studentRace", "studentRace",
                        RequiredFieldMetadata("raceType", "string"))),

                FieldMetadata("telephones", "array",
                    FieldMetadata("studentTelephone", "studentTelephone",
                        RequiredFieldMetadata("telephoneNumberType", "string"),
                        RequiredFieldMetadata("telephoneNumber", "string"),
                        FieldMetadata("orderOfPriority", "integer"),
                        FieldMetadata("textMessageCapabilityIndicator", "boolean"))),

                FieldMetadata("visas", "array",
                    FieldMetadata("studentVisa", "studentVisa",
                        RequiredFieldMetadata("visaType", "string"))));

            response.Mappings.ShouldMatch(
                Field("birthCity"),
                Field("birthCountryDescriptor"),
                Field("birthDate"),
                Field("birthInternationalProvince"),
                Field("birthStateAbbreviationType"),
                Field("citizenshipStatusType"),
                Field("dateEnteredUS"),
                Field("displacementStatus"),
                Field("economicDisadvantaged"),
                Field("firstName"),
                Field("generationCodeSuffix"),
                Field("hispanicLatinoEthnicity"),
                Field("lastSurname"),
                Field("limitedEnglishProficiencyDescriptor"),
                Field("loginId"),
                Field("maidenName"),
                Field("middleName"),
                Field("multipleBirthStatus"),
                Field("oldEthnicityType"),
                Field("personalTitlePrefix"),
                Field("profileThumbnail"),
                Field("schoolFoodServicesEligibilityDescriptor"),
                Field("sexType"),
                Field("studentUniqueId"),
                Field("learningStyle",
                    Field("visualLearning"),
                    Field("auditoryLearning"),
                    Field("tactileLearning")),
                Field("addresses"),
                Field("characteristics"),
                Field("cohortYears"),
                Field("disabilities"),
                Field("electronicMails"),
                Field("identificationCodes"),
                Field("identificationDocuments"),
                Field("indicators"),
                Field("internationalAddresses"),
                Field("languages"),
                Field("otherNames"),
                Field("programParticipations"),
                Field("races"),
                Field("telephones"),
                Field("visas"));
        }

        [Test]
        public override async Task ShouldGetUserFacingMappableFieldsForStudentAssessmentsResource()
        {
            var response = await GetDataMapperFields("/studentAssessments");

            response.ResourceMetadata.ShouldMatch(

                RequiredFieldMetadata("assessmentReference", "assessmentReference",
                    RequiredFieldMetadata("identifier", "string"),
                    RequiredFieldMetadata("namespace", "string")),

                RequiredFieldMetadata("studentReference", "studentReference",
                    RequiredFieldMetadata("studentUniqueId", "string")),

                RequiredFieldMetadata("administrationDate", "string"),
                FieldMetadata("administrationEndDate", "string"),
                FieldMetadata("administrationEnvironmentType", "string"),
                FieldMetadata("administrationLanguageDescriptor", "string"),
                FieldMetadata("eventCircumstanceType", "string"),
                FieldMetadata("eventDescription", "string"),
                FieldMetadata("reasonNotTestedType", "string"),
                FieldMetadata("retestIndicatorType", "string"),
                FieldMetadata("serialNumber", "string"),
                RequiredFieldMetadata("identifier", "string"),
                FieldMetadata("whenAssessedGradeLevelDescriptor", "string"),

                FieldMetadata("accommodations", "array",
                    FieldMetadata("studentAssessmentAccommodation", "studentAssessmentAccommodation",
                        RequiredFieldMetadata("accommodationDescriptor", "string"))),

                FieldMetadata("items", "array",
                    FieldMetadata("studentAssessmentItem", "studentAssessmentItem",
                        RequiredFieldMetadata("assessmentItemReference", "assessmentItemReference",
                            RequiredFieldMetadata("assessmentIdentifier", "string"),
                            RequiredFieldMetadata("identificationCode", "string"),
                            RequiredFieldMetadata("namespace", "string")),
                        RequiredFieldMetadata("assessmentItemResultType", "string"),
                        FieldMetadata("responseIndicatorType", "string"),
                        FieldMetadata("assessmentResponse", "string"),
                        FieldMetadata("descriptiveFeedback", "string"),
                        FieldMetadata("rawScoreResult", "integer"),
                        FieldMetadata("timeAssessed", "string"))),

                FieldMetadata("performanceLevels", "array",
                    FieldMetadata("studentAssessmentPerformanceLevel", "studentAssessmentPerformanceLevel",
                        RequiredFieldMetadata("assessmentReportingMethodType", "string"),
                        RequiredFieldMetadata("performanceLevelDescriptor", "string"),
                        RequiredFieldMetadata("performanceLevelMet", "boolean"))),

                FieldMetadata("scoreResults", "array",
                    FieldMetadata("studentAssessmentScoreResult", "studentAssessmentScoreResult",
                        RequiredFieldMetadata("assessmentReportingMethodType", "string"),
                        RequiredFieldMetadata("resultDatatypeType", "string"),
                        RequiredFieldMetadata("result", "string"))),

                FieldMetadata("studentObjectiveAssessments", "array",
                    FieldMetadata("studentAssessmentStudentObjectiveAssessment", "studentAssessmentStudentObjectiveAssessment",
                        RequiredFieldMetadata("objectiveAssessmentReference", "objectiveAssessmentReference",
                            RequiredFieldMetadata("assessmentIdentifier", "string"),
                            RequiredFieldMetadata("identificationCode", "string"),
                            RequiredFieldMetadata("namespace", "string")),
                        FieldMetadata("performanceLevels", "array",
                            FieldMetadata("studentAssessmentStudentObjectiveAssessmentPerformanceLevel", "studentAssessmentStudentObjectiveAssessmentPerformanceLevel",
                                RequiredFieldMetadata("assessmentReportingMethodType", "string"),
                                RequiredFieldMetadata("performanceLevelDescriptor", "string"),
                                RequiredFieldMetadata("performanceLevelMet", "boolean"))),
                        RequiredFieldMetadata("scoreResults", "array",
                            FieldMetadata("studentAssessmentStudentObjectiveAssessmentScoreResult", "studentAssessmentStudentObjectiveAssessmentScoreResult",
                                RequiredFieldMetadata("assessmentReportingMethodType", "string"),
                                RequiredFieldMetadata("resultDatatypeType", "string"),
                                RequiredFieldMetadata("result", "string")))))

            );

            response.Mappings.ShouldMatch(

                Field("assessmentReference",
                    Field("identifier"),
                    Field("namespace")),

                Field("studentReference",
                    Field("studentUniqueId")),

                Field("administrationDate"),
                Field("administrationEndDate"),
                Field("administrationEnvironmentType"),
                Field("administrationLanguageDescriptor"),
                Field("eventCircumstanceType"),
                Field("eventDescription"),
                Field("reasonNotTestedType"),
                Field("retestIndicatorType"),
                Field("serialNumber"),
                Field("identifier"),
                Field("whenAssessedGradeLevelDescriptor"),

                // Array mappings begin with zero items.
                Field("accommodations"),
                Field("items"),
                Field("performanceLevels"),
                Field("scoreResults"),
                Field("studentObjectiveAssessments")
            );
        }
    }
}
