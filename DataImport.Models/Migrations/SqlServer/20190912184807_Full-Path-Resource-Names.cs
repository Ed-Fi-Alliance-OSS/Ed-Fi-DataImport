// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using Microsoft.EntityFrameworkCore.Migrations;

namespace DataImport.Models.Migrations.SqlServer
{
    public partial class FullPathResourceNames : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Resources.Path contains the truly unique and fully-descriptive path
            // for all resources. Prior to this migration, the following columns
            // contain a shortened, unique nickname for those paths:
            //
            //      Resources.Name
            //      BootstrapDatas.ResourceName
            //      DataMaps.ResourceName
            //
            // Although unique for any successfully-configured target ODS, these
            // shortened names are insufficient against arbitrary possible target ODS's,
            // and need to be corrected back to the full Path string, so that resources
            // can truly be distinguished from one another in the event of custom
            // extensions with similar-named resources.
            //
            // This migration restores those 3 name columns to contain the full Path
            // as would appear in Resources.Path. Note, though, that in a system where
            // the user has pointed at different ODS instances/versions over time, some
            // records may not be perfectly corrected here. For some orphaned records,
            // where BootstrapDatas/DataMaps' ResourceName is no longer present in the
            // current Resources cache table and where the names are not recognized as
            // core Ed-Fi resources, no perfect correction can be made. In these cases,
            // the names are updated with an attention-getting prefix, to provoke support
            // requests to ultimately fix the records manually.

            // First, mark ALL orphaned records' ResourceName columns so that they can
            // be recognized throughout the migration. In most real-world scenarios,
            // there are no such records.
            MarkOrphanedRecordResourceNames(migrationBuilder);

            // Next, save full Paths to all DataMaps and Bootstraps corresponding
            // with the currently-configured target ODS. The vast majority of real-world
            // scenarios are resolved by these queries.
            migrationBuilder.Sql(
                "UPDATE B " +
                "SET ResourceName = (SELECT R.Path FROM dbo.Resources R WHERE R.Name = B.ResourceName) " +
                "FROM dbo.BootstrapDatas B " +
                "WHERE ResourceName IN (SELECT Name FROM dbo.Resources)");

            migrationBuilder.Sql(
                "UPDATE D " +
                "SET ResourceName = (SELECT R.Path FROM dbo.Resources R WHERE R.Name = D.ResourceName) " +
                "FROM dbo.DataMaps D " +
                "WHERE ResourceName IN (SELECT Name FROM dbo.Resources)");

            // Next, based on well-known 2.5 and 3.1.1 metadata, correct records that are
            // orphaned from the current Resources cache but that are recognizable by name
            // as core Ed-Fi resources. In most real-world scenarios, there are no such
            // records.
            FixOrphanedRecordResourceNames(migrationBuilder);

            // Now that child tables use the full Path as the ResourceName, store the same
            // values in the Resources.Name column. This renders the Resources.Path column
            // redundant until it can be safely dropped.
            migrationBuilder.Sql("UPDATE dbo.Resources SET Name = Path");
        }

        private static void MarkOrphanedRecordResourceNames(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "BootstrapDatas", "DataMaps" })
                migrationBuilder.Sql(
                    $"UPDATE dbo.{table} " +
                    "SET ResourceName = '(Full Path Unknown) ' + ResourceName " +
                    "WHERE ResourceName NOT IN (SELECT Name FROM dbo.Resources)");
        }

        private static void FixOrphanedRecordResourceNames(MigrationBuilder migrationBuilder)
        {
            // Well-Known 3.1.1 Resource Name Corrections
            FixOrphanedRecordResourceNames(migrationBuilder, "academicWeek", "/ed-fi/academicWeeks");
            FixOrphanedRecordResourceNames(migrationBuilder, "account", "/ed-fi/accounts");
            FixOrphanedRecordResourceNames(migrationBuilder, "accountabilityRating", "/ed-fi/accountabilityRatings");
            FixOrphanedRecordResourceNames(migrationBuilder, "accountCode", "/ed-fi/accountCodes");
            FixOrphanedRecordResourceNames(migrationBuilder, "actual", "/ed-fi/actuals");
            FixOrphanedRecordResourceNames(migrationBuilder, "applicant", "/grand-bend/applicants");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessment", "/ed-fi/assessments");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentItem", "/ed-fi/assessmentItems");
            FixOrphanedRecordResourceNames(migrationBuilder, "bellSchedule", "/ed-fi/bellSchedules");
            FixOrphanedRecordResourceNames(migrationBuilder, "budget", "/ed-fi/budgets");
            FixOrphanedRecordResourceNames(migrationBuilder, "calendar", "/ed-fi/calendars");
            FixOrphanedRecordResourceNames(migrationBuilder, "calendarDate", "/ed-fi/calendarDates");
            FixOrphanedRecordResourceNames(migrationBuilder, "classPeriod", "/ed-fi/classPeriods");
            FixOrphanedRecordResourceNames(migrationBuilder, "cohort", "/ed-fi/cohorts");
            FixOrphanedRecordResourceNames(migrationBuilder, "communityOrganization", "/ed-fi/communityOrganizations");
            FixOrphanedRecordResourceNames(migrationBuilder, "communityProvider", "/ed-fi/communityProviders");
            FixOrphanedRecordResourceNames(migrationBuilder, "communityProviderLicense", "/ed-fi/communityProviderLicenses");
            FixOrphanedRecordResourceNames(migrationBuilder, "competencyObjective", "/ed-fi/competencyObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "contractedStaff", "/ed-fi/contractedStaffs");
            FixOrphanedRecordResourceNames(migrationBuilder, "course", "/ed-fi/courses");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseOffering", "/ed-fi/courseOfferings");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseTranscript", "/ed-fi/courseTranscripts");
            FixOrphanedRecordResourceNames(migrationBuilder, "credential", "/ed-fi/credentials");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineAction", "/ed-fi/disciplineActions");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineIncident", "/ed-fi/disciplineIncidents");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationContent", "/ed-fi/educationContents");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationInterventionPrescriptionAssociation", "/ed-fi/educationOrganizationInterventionPrescriptionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationNetwork", "/ed-fi/educationOrganizationNetworks");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationNetworkAssociation", "/ed-fi/educationOrganizationNetworkAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationPeerAssociation", "/ed-fi/educationOrganizationPeerAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationServiceCenter", "/ed-fi/educationServiceCenters");
            FixOrphanedRecordResourceNames(migrationBuilder, "feederSchoolAssociation", "/ed-fi/feederSchoolAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "grade", "/ed-fi/grades");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradebookEntry", "/ed-fi/gradebookEntries");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradingPeriod", "/ed-fi/gradingPeriods");
            FixOrphanedRecordResourceNames(migrationBuilder, "graduationPlan", "/ed-fi/graduationPlans");
            FixOrphanedRecordResourceNames(migrationBuilder, "intervention", "/ed-fi/interventions");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventionPrescription", "/ed-fi/interventionPrescriptions");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventionStudy", "/ed-fi/interventionStudies");
            FixOrphanedRecordResourceNames(migrationBuilder, "learningObjective", "/ed-fi/learningObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "learningStandard", "/ed-fi/learningStandards");
            FixOrphanedRecordResourceNames(migrationBuilder, "localEducationAgency", "/ed-fi/localEducationAgencies");
            FixOrphanedRecordResourceNames(migrationBuilder, "location", "/ed-fi/locations");
            FixOrphanedRecordResourceNames(migrationBuilder, "objectiveAssessment", "/ed-fi/objectiveAssessments");
            FixOrphanedRecordResourceNames(migrationBuilder, "openStaffPosition", "/ed-fi/openStaffPositions");
            FixOrphanedRecordResourceNames(migrationBuilder, "parent", "/ed-fi/parents");
            FixOrphanedRecordResourceNames(migrationBuilder, "payroll", "/ed-fi/payrolls");
            FixOrphanedRecordResourceNames(migrationBuilder, "postSecondaryEvent", "/ed-fi/postSecondaryEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "postSecondaryInstitution", "/ed-fi/postSecondaryInstitutions");
            FixOrphanedRecordResourceNames(migrationBuilder, "program", "/ed-fi/programs");
            FixOrphanedRecordResourceNames(migrationBuilder, "reportCard", "/ed-fi/reportCards");
            FixOrphanedRecordResourceNames(migrationBuilder, "restraintEvent", "/ed-fi/restraintEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "school", "/ed-fi/schools");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolYearType", "/ed-fi/schoolYearTypes");
            FixOrphanedRecordResourceNames(migrationBuilder, "section", "/ed-fi/sections");
            FixOrphanedRecordResourceNames(migrationBuilder, "sectionAttendanceTakenEvent", "/ed-fi/sectionAttendanceTakenEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "session", "/ed-fi/sessions");
            FixOrphanedRecordResourceNames(migrationBuilder, "staff", "/ed-fi/staffs");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffAbsenceEvent", "/ed-fi/staffAbsenceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffCohortAssociation", "/ed-fi/staffCohortAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffEducationOrganizationAssignmentAssociation", "/ed-fi/staffEducationOrganizationAssignmentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffEducationOrganizationContactAssociation", "/ed-fi/staffEducationOrganizationContactAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffEducationOrganizationEmploymentAssociation", "/ed-fi/staffEducationOrganizationEmploymentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffLeave", "/ed-fi/staffLeaves");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffProgramAssociation", "/ed-fi/staffProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffSchoolAssociation", "/ed-fi/staffSchoolAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffSectionAssociation", "/ed-fi/staffSectionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "stateEducationAgency", "/ed-fi/stateEducationAgencies");
            FixOrphanedRecordResourceNames(migrationBuilder, "student", "/ed-fi/students");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentAcademicRecord", "/ed-fi/studentAcademicRecords");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentAssessment", "/ed-fi/studentAssessments");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCohortAssociation", "/ed-fi/studentCohortAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCompetencyObjective", "/ed-fi/studentCompetencyObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCTEProgramAssociation", "/ed-fi/studentCTEProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentDisciplineIncidentAssociation", "/ed-fi/studentDisciplineIncidentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentEducationOrganizationAssociation", "/ed-fi/studentEducationOrganizationAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentEducationOrganizationResponsibilityAssociation", "/ed-fi/studentEducationOrganizationResponsibilityAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentGradebookEntry", "/ed-fi/studentGradebookEntries");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentHomelessProgramAssociation", "/ed-fi/studentHomelessProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentInterventionAssociation", "/ed-fi/studentInterventionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentInterventionAttendanceEvent", "/ed-fi/studentInterventionAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentLanguageInstructionProgramAssociation", "/ed-fi/studentLanguageInstructionProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentLearningObjective", "/ed-fi/studentLearningObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentMigrantEducationProgramAssociation", "/ed-fi/studentMigrantEducationProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentNeglectedOrDelinquentProgramAssociation", "/ed-fi/studentNeglectedOrDelinquentProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentParentAssociation", "/ed-fi/studentParentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentProgramAssociation", "/ed-fi/studentProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentProgramAttendanceEvent", "/ed-fi/studentProgramAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSchoolAssociation", "/ed-fi/studentSchoolAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSchoolAttendanceEvent", "/ed-fi/studentSchoolAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSchoolFoodServiceProgramAssociation", "/ed-fi/studentSchoolFoodServiceProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSectionAssociation", "/ed-fi/studentSectionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSectionAttendanceEvent", "/ed-fi/studentSectionAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSpecialEducationProgramAssociation", "/ed-fi/studentSpecialEducationProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentTitleIPartAProgramAssociation", "/ed-fi/studentTitleIPartAProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "absenceEventCategoryDescriptor", "/ed-fi/absenceEventCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "academicHonorCategoryDescriptor", "/ed-fi/academicHonorCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "academicSubjectDescriptor", "/ed-fi/academicSubjectDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "accommodationDescriptor", "/ed-fi/accommodationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "accountClassificationDescriptor", "/ed-fi/accountClassificationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "achievementCategoryDescriptor", "/ed-fi/achievementCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "additionalCreditTypeDescriptor", "/ed-fi/additionalCreditTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "addressTypeDescriptor", "/ed-fi/addressTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "administrationEnvironmentDescriptor", "/ed-fi/administrationEnvironmentDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "administrativeFundingControlDescriptor", "/ed-fi/administrativeFundingControlDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentCategoryDescriptor", "/ed-fi/assessmentCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentIdentificationSystemDescriptor", "/ed-fi/assessmentIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentItemCategoryDescriptor", "/ed-fi/assessmentItemCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentItemResultDescriptor", "/ed-fi/assessmentItemResultDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentPeriodDescriptor", "/ed-fi/assessmentPeriodDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentReportingMethodDescriptor", "/ed-fi/assessmentReportingMethodDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "attemptStatusDescriptor", "/ed-fi/attemptStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "attendanceEventCategoryDescriptor", "/ed-fi/attendanceEventCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "behaviorDescriptor", "/ed-fi/behaviorDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "calendarEventDescriptor", "/ed-fi/calendarEventDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "calendarTypeDescriptor", "/ed-fi/calendarTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "careerPathwayDescriptor", "/ed-fi/careerPathwayDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "charterApprovalAgencyTypeDescriptor", "/ed-fi/charterApprovalAgencyTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "charterStatusDescriptor", "/ed-fi/charterStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "citizenshipStatusDescriptor", "/ed-fi/citizenshipStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "classroomPositionDescriptor", "/ed-fi/classroomPositionDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "cohortScopeDescriptor", "/ed-fi/cohortScopeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "cohortTypeDescriptor", "/ed-fi/cohortTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "cohortYearTypeDescriptor", "/ed-fi/cohortYearTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "competencyLevelDescriptor", "/ed-fi/competencyLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "contactTypeDescriptor", "/ed-fi/contactTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "contentClassDescriptor", "/ed-fi/contentClassDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "continuationOfServicesReasonDescriptor", "/ed-fi/continuationOfServicesReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "costRateDescriptor", "/ed-fi/costRateDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "countryDescriptor", "/ed-fi/countryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseAttemptResultDescriptor", "/ed-fi/courseAttemptResultDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseDefinedByDescriptor", "/ed-fi/courseDefinedByDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseGPAApplicabilityDescriptor", "/ed-fi/courseGPAApplicabilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseIdentificationSystemDescriptor", "/ed-fi/courseIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseLevelCharacteristicDescriptor", "/ed-fi/courseLevelCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseRepeatCodeDescriptor", "/ed-fi/courseRepeatCodeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "credentialFieldDescriptor", "/ed-fi/credentialFieldDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "credentialTypeDescriptor", "/ed-fi/credentialTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "creditTypeDescriptor", "/ed-fi/creditTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "curriculumUsedDescriptor", "/ed-fi/curriculumUsedDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "deliveryMethodDescriptor", "/ed-fi/deliveryMethodDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "diagnosisDescriptor", "/ed-fi/diagnosisDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "diplomaLevelDescriptor", "/ed-fi/diplomaLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "diplomaTypeDescriptor", "/ed-fi/diplomaTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disabilityDescriptor", "/ed-fi/disabilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disabilityDesignationDescriptor", "/ed-fi/disabilityDesignationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disabilityDeterminationSourceTypeDescriptor", "/ed-fi/disabilityDeterminationSourceTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineActionLengthDifferenceReasonDescriptor", "/ed-fi/disciplineActionLengthDifferenceReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineDescriptor", "/ed-fi/disciplineDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationalEnvironmentDescriptor", "/ed-fi/educationalEnvironmentDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationCategoryDescriptor", "/ed-fi/educationOrganizationCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationIdentificationSystemDescriptor", "/ed-fi/educationOrganizationIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationPlanDescriptor", "/ed-fi/educationPlanDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "electronicMailTypeDescriptor", "/ed-fi/electronicMailTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "employmentStatusDescriptor", "/ed-fi/employmentStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "entryGradeLevelReasonDescriptor", "/ed-fi/entryGradeLevelReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "entryTypeDescriptor", "/ed-fi/entryTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "eventCircumstanceDescriptor", "/ed-fi/eventCircumstanceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "exitWithdrawTypeDescriptor", "/ed-fi/exitWithdrawTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradebookEntryTypeDescriptor", "/ed-fi/gradebookEntryTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradeLevelDescriptor", "/ed-fi/gradeLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradeTypeDescriptor", "/ed-fi/gradeTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradingPeriodDescriptor", "/ed-fi/gradingPeriodDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "graduationPlanTypeDescriptor", "/ed-fi/graduationPlanTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gunFreeSchoolsActReportingStatusDescriptor", "/ed-fi/gunFreeSchoolsActReportingStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "homelessPrimaryNighttimeResidenceDescriptor", "/ed-fi/homelessPrimaryNighttimeResidenceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "homelessProgramServiceDescriptor", "/ed-fi/homelessProgramServiceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "identificationDocumentUseDescriptor", "/ed-fi/identificationDocumentUseDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "incidentLocationDescriptor", "/ed-fi/incidentLocationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "institutionTelephoneNumberTypeDescriptor", "/ed-fi/institutionTelephoneNumberTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "interactivityStyleDescriptor", "/ed-fi/interactivityStyleDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "internetAccessDescriptor", "/ed-fi/internetAccessDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventionClassDescriptor", "/ed-fi/interventionClassDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventionEffectivenessRatingDescriptor", "/ed-fi/interventionEffectivenessRatingDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "languageDescriptor", "/ed-fi/languageDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "languageInstructionProgramServiceDescriptor", "/ed-fi/languageInstructionProgramServiceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "languageUseDescriptor", "/ed-fi/languageUseDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "learningStandardCategoryDescriptor", "/ed-fi/learningStandardCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "levelOfEducationDescriptor", "/ed-fi/levelOfEducationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "licenseStatusDescriptor", "/ed-fi/licenseStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "licenseTypeDescriptor", "/ed-fi/licenseTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "limitedEnglishProficiencyDescriptor", "/ed-fi/limitedEnglishProficiencyDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "localeDescriptor", "/ed-fi/localeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "localEducationAgencyCategoryDescriptor", "/ed-fi/localEducationAgencyCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "magnetSpecialProgramEmphasisSchoolDescriptor", "/ed-fi/magnetSpecialProgramEmphasisSchoolDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "mediumOfInstructionDescriptor", "/ed-fi/mediumOfInstructionDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "methodCreditEarnedDescriptor", "/ed-fi/methodCreditEarnedDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "migrantEducationProgramServiceDescriptor", "/ed-fi/migrantEducationProgramServiceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "monitoredDescriptor", "/ed-fi/monitoredDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "neglectedOrDelinquentProgramDescriptor", "/ed-fi/neglectedOrDelinquentProgramDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "neglectedOrDelinquentProgramServiceDescriptor", "/ed-fi/neglectedOrDelinquentProgramServiceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "networkPurposeDescriptor", "/ed-fi/networkPurposeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "oldEthnicityDescriptor", "/ed-fi/oldEthnicityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "operationalStatusDescriptor", "/ed-fi/operationalStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "otherNameTypeDescriptor", "/ed-fi/otherNameTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "participationDescriptor", "/ed-fi/participationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "performanceBaseConversionDescriptor", "/ed-fi/performanceBaseConversionDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "performanceLevelDescriptor", "/ed-fi/performanceLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "personalInformationVerificationDescriptor", "/ed-fi/personalInformationVerificationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "populationServedDescriptor", "/ed-fi/populationServedDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "postingResultDescriptor", "/ed-fi/postingResultDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "postSecondaryEventCategoryDescriptor", "/ed-fi/postSecondaryEventCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "postSecondaryInstitutionLevelDescriptor", "/ed-fi/postSecondaryInstitutionLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "proficiencyDescriptor", "/ed-fi/proficiencyDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "programAssignmentDescriptor", "/ed-fi/programAssignmentDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "programCharacteristicDescriptor", "/ed-fi/programCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "programSponsorDescriptor", "/ed-fi/programSponsorDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "programTypeDescriptor", "/ed-fi/programTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "progressDescriptor", "/ed-fi/progressDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "progressLevelDescriptor", "/ed-fi/progressLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "providerCategoryDescriptor", "/ed-fi/providerCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "providerProfitabilityDescriptor", "/ed-fi/providerProfitabilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "providerStatusDescriptor", "/ed-fi/providerStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "publicationStatusDescriptor", "/ed-fi/publicationStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "raceDescriptor", "/ed-fi/raceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "reasonExitedDescriptor", "/ed-fi/reasonExitedDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "reasonNotTestedDescriptor", "/ed-fi/reasonNotTestedDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "recognitionTypeDescriptor", "/ed-fi/recognitionTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "relationDescriptor", "/ed-fi/relationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "repeatIdentifierDescriptor", "/ed-fi/repeatIdentifierDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "reporterDescriptionDescriptor", "/ed-fi/reporterDescriptionDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "residencyStatusDescriptor", "/ed-fi/residencyStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "responseIndicatorDescriptor", "/ed-fi/responseIndicatorDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "responsibilityDescriptor", "/ed-fi/responsibilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "restraintEventReasonDescriptor", "/ed-fi/restraintEventReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "resultDatatypeTypeDescriptor", "/ed-fi/resultDatatypeTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "retestIndicatorDescriptor", "/ed-fi/retestIndicatorDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolCategoryDescriptor", "/ed-fi/schoolCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolChoiceImplementStatusDescriptor", "/ed-fi/schoolChoiceImplementStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolFoodServiceProgramServiceDescriptor", "/ed-fi/schoolFoodServiceProgramServiceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolTypeDescriptor", "/ed-fi/schoolTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "sectionCharacteristicDescriptor", "/ed-fi/sectionCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "separationDescriptor", "/ed-fi/separationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "separationReasonDescriptor", "/ed-fi/separationReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "serviceDescriptor", "/ed-fi/serviceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "sexDescriptor", "/ed-fi/sexDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "specialEducationProgramServiceDescriptor", "/ed-fi/specialEducationProgramServiceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "specialEducationSettingDescriptor", "/ed-fi/specialEducationSettingDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffClassificationDescriptor", "/ed-fi/staffClassificationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffIdentificationSystemDescriptor", "/ed-fi/staffIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffLeaveEventCategoryDescriptor", "/ed-fi/staffLeaveEventCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "stateAbbreviationDescriptor", "/ed-fi/stateAbbreviationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCharacteristicDescriptor", "/ed-fi/studentCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentIdentificationSystemDescriptor", "/ed-fi/studentIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentParticipationCodeDescriptor", "/ed-fi/studentParticipationCodeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "teachingCredentialBasisDescriptor", "/ed-fi/teachingCredentialBasisDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "teachingCredentialDescriptor", "/ed-fi/teachingCredentialDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "technicalSkillsAssessmentDescriptor", "/ed-fi/technicalSkillsAssessmentDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "telephoneNumberTypeDescriptor", "/ed-fi/telephoneNumberTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "termDescriptor", "/ed-fi/termDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "titleIPartAParticipantDescriptor", "/ed-fi/titleIPartAParticipantDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "titleIPartASchoolDesignationDescriptor", "/ed-fi/titleIPartASchoolDesignationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "tribalAffiliationDescriptor", "/ed-fi/tribalAffiliationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "visaDescriptor", "/ed-fi/visaDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "weaponDescriptor", "/ed-fi/weaponDescriptors");

            // Well-Known 2.5 Resource Name Corrections
            FixOrphanedRecordResourceNames(migrationBuilder, "academicWeeks", "/academicWeeks");
            FixOrphanedRecordResourceNames(migrationBuilder, "accountabilityRatings", "/accountabilityRatings");
            FixOrphanedRecordResourceNames(migrationBuilder, "accounts", "/accounts");
            FixOrphanedRecordResourceNames(migrationBuilder, "actuals", "/actuals");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentFamilies", "/assessmentFamilies");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentItems", "/assessmentItems");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessments", "/assessments");
            FixOrphanedRecordResourceNames(migrationBuilder, "bellSchedules", "/bellSchedules");
            FixOrphanedRecordResourceNames(migrationBuilder, "budgets", "/budgets");
            FixOrphanedRecordResourceNames(migrationBuilder, "calendarDates", "/calendarDates");
            FixOrphanedRecordResourceNames(migrationBuilder, "classPeriods", "/classPeriods");
            FixOrphanedRecordResourceNames(migrationBuilder, "cohorts", "/cohorts");
            FixOrphanedRecordResourceNames(migrationBuilder, "competencyObjectives", "/competencyObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "contractedStaffs", "/contractedStaffs");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseOfferings", "/courseOfferings");
            FixOrphanedRecordResourceNames(migrationBuilder, "courses", "/courses");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseTranscripts", "/courseTranscripts");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineActions", "/disciplineActions");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineIncidents", "/disciplineIncidents");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationContents", "/educationContents");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationInterventionPrescriptionAssociations", "/educationOrganizationInterventionPrescriptionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationNetworkAssociations", "/educationOrganizationNetworkAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationNetworks", "/educationOrganizationNetworks");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationPeerAssociations", "/educationOrganizationPeerAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationServiceCenters", "/educationServiceCenters");
            FixOrphanedRecordResourceNames(migrationBuilder, "feederSchoolAssociations", "/feederSchoolAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradebookEntries", "/gradebookEntries");
            FixOrphanedRecordResourceNames(migrationBuilder, "grades", "/grades");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradingPeriods", "/gradingPeriods");
            FixOrphanedRecordResourceNames(migrationBuilder, "graduationPlans", "/graduationPlans");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventionPrescriptions", "/interventionPrescriptions");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventions", "/interventions");
            FixOrphanedRecordResourceNames(migrationBuilder, "interventionStudies", "/interventionStudies");
            FixOrphanedRecordResourceNames(migrationBuilder, "learningObjectives", "/learningObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "learningStandards", "/learningStandards");
            FixOrphanedRecordResourceNames(migrationBuilder, "leaveEvents", "/leaveEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "localEducationAgencies", "/localEducationAgencies");
            FixOrphanedRecordResourceNames(migrationBuilder, "locations", "/locations");
            FixOrphanedRecordResourceNames(migrationBuilder, "objectiveAssessments", "/objectiveAssessments");
            FixOrphanedRecordResourceNames(migrationBuilder, "openStaffPositions", "/openStaffPositions");
            FixOrphanedRecordResourceNames(migrationBuilder, "parents", "/parents");
            FixOrphanedRecordResourceNames(migrationBuilder, "payrolls", "/payrolls");
            FixOrphanedRecordResourceNames(migrationBuilder, "postSecondaryEvents", "/postSecondaryEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "programs", "/programs");
            FixOrphanedRecordResourceNames(migrationBuilder, "reportCards", "/reportCards");
            FixOrphanedRecordResourceNames(migrationBuilder, "restraintEvents", "/restraintEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "schools", "/schools");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolYearTypes", "/schoolYearTypes");
            FixOrphanedRecordResourceNames(migrationBuilder, "sectionAttendanceTakenEvents", "/sectionAttendanceTakenEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "sections", "/sections");
            FixOrphanedRecordResourceNames(migrationBuilder, "sessions", "/sessions");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffCohortAssociations", "/staffCohortAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffEducationOrganizationAssignmentAssociations", "/staffEducationOrganizationAssignmentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffEducationOrganizationEmploymentAssociations", "/staffEducationOrganizationEmploymentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffProgramAssociations", "/staffProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffs", "/staffs");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffSchoolAssociations", "/staffSchoolAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffSectionAssociations", "/staffSectionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "stateEducationAgencies", "/stateEducationAgencies");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentAcademicRecords", "/studentAcademicRecords");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentAssessments", "/studentAssessments");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCohortAssociations", "/studentCohortAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCompetencyObjectives", "/studentCompetencyObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCTEProgramAssociations", "/studentCTEProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentDisciplineIncidentAssociations", "/studentDisciplineIncidentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentEducationOrganizationAssociations", "/studentEducationOrganizationAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentGradebookEntries", "/studentGradebookEntries");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentInterventionAssociations", "/studentInterventionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentInterventionAttendanceEvents", "/studentInterventionAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentLearningObjectives", "/studentLearningObjectives");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentMigrantEducationProgramAssociations", "/studentMigrantEducationProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentParentAssociations", "/studentParentAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentProgramAssociations", "/studentProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentProgramAttendanceEvents", "/studentProgramAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "students", "/students");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSchoolAssociations", "/studentSchoolAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSchoolAttendanceEvents", "/studentSchoolAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSectionAssociations", "/studentSectionAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSectionAttendanceEvents", "/studentSectionAttendanceEvents");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentSpecialEducationProgramAssociations", "/studentSpecialEducationProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentTitleIPartAProgramAssociations", "/studentTitleIPartAProgramAssociations");
            FixOrphanedRecordResourceNames(migrationBuilder, "academicSubjectDescriptors", "/academicSubjectDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "accommodationDescriptors", "/accommodationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "accountCodeDescriptors", "/accountCodeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "achievementCategoryDescriptors", "/achievementCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "administrativeFundingControlDescriptors", "/administrativeFundingControlDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentCategoryDescriptors", "/assessmentCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentIdentificationSystemDescriptors", "/assessmentIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "assessmentPeriodDescriptors", "/assessmentPeriodDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "attendanceEventCategoryDescriptors", "/attendanceEventCategoryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "behaviorDescriptors", "/behaviorDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "calendarEventDescriptors", "/calendarEventDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "classroomPositionDescriptors", "/classroomPositionDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "competencyLevelDescriptors", "/competencyLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "continuationOfServicesReasonDescriptors", "/continuationOfServicesReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "countryDescriptors", "/countryDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "courseIdentificationSystemDescriptors", "/courseIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "credentialFieldDescriptors", "/credentialFieldDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "diagnosisDescriptors", "/diagnosisDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disabilityDescriptors", "/disabilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "disciplineDescriptors", "/disciplineDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "educationOrganizationIdentificationSystemDescriptors", "/educationOrganizationIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "employmentStatusDescriptors", "/employmentStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "entryTypeDescriptors", "/entryTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "exitWithdrawTypeDescriptors", "/exitWithdrawTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradeLevelDescriptors", "/gradeLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "gradingPeriodDescriptors", "/gradingPeriodDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "graduationPlanTypeDescriptors", "/graduationPlanTypeDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "languageDescriptors", "/languageDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "levelDescriptors", "/levelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "levelOfEducationDescriptors", "/levelOfEducationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "limitedEnglishProficiencyDescriptors", "/limitedEnglishProficiencyDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "performanceLevelDescriptors", "/performanceLevelDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "programAssignmentDescriptors", "/programAssignmentDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "programCharacteristicDescriptors", "/programCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "reasonExitedDescriptors", "/reasonExitedDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "reporterDescriptionDescriptors", "/reporterDescriptionDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "residencyStatusDescriptors", "/residencyStatusDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "responsibilityDescriptors", "/responsibilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "schoolFoodServicesEligibilityDescriptors", "/schoolFoodServicesEligibilityDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "sectionCharacteristicDescriptors", "/sectionCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "separationReasonDescriptors", "/separationReasonDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "serviceDescriptors", "/serviceDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "specialEducationSettingDescriptors", "/specialEducationSettingDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffClassificationDescriptors", "/staffClassificationDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "staffIdentificationSystemDescriptors", "/staffIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentCharacteristicDescriptors", "/studentCharacteristicDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "studentIdentificationSystemDescriptors", "/studentIdentificationSystemDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "teachingCredentialDescriptors", "/teachingCredentialDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "termDescriptors", "/termDescriptors");
            FixOrphanedRecordResourceNames(migrationBuilder, "weaponDescriptors", "/weaponDescriptors");
        }

        private static void FixOrphanedRecordResourceNames(MigrationBuilder migrationBuilder, string insufficientName, string fullName)
        {
            foreach (var table in new[] { "BootstrapDatas", "DataMaps" })
                migrationBuilder.Sql(
                    $"UPDATE dbo.{table} " +
                    $"SET ResourceName = '{fullName}' " +
                    $"WHERE ResourceName = '(Full Path Unknown) ' + '{insufficientName}' ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
