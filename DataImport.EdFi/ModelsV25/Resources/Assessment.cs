// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System.Collections.Generic;

namespace DataImport.EdFi.ModelsV25.Resources
{
    public class Assessment
    {
        public string Id { get; set; }

        public string CategoryDescriptor { get; set; }

        public string Identifier { get; set; }

        public string Title { get; set; }

        public string Namespace { get; set; }

        public int? Version { get; set; }

        public List<AssessmentAcademicSubject> AcademicSubjects { get; set; }

        public List<AssessmentAssessedGradeLevel> AssessedGradeLevels { get; set; }

        public List<AssessmentIdentificationCode> IdentificationCodes { get; set; }

        public List<AssessmentPerformanceLevel> PerformanceLevels { get; set; }
    }

    public class AssessmentAcademicSubject
    {
        public string AcademicSubjectDescriptor { get; set; }
    }

    public class AssessmentAssessedGradeLevel
    {
        public string GradeLevelDescriptor { get; set; }
    }

    public class AssessmentIdentificationCode
    {
        public string AssessmentIdentificationSystemDescriptor { get; set; }
    }

    public class AssessmentPerformanceLevel
    {
        public string AssessmentReportingMethodType { get; set; }

        public string PerformanceLevelDescriptor { get; set; }

        public string ResultDatatypeType { get; set; }

        public string MinimumScore { get; set; }

        public string MaximumScore { get; set; }
    }
}
