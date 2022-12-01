// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

namespace DataImport.EdFi.ModelsV25.EnrollmentComposite
{
    public class Section
    {
        public string UniqueSectionCode { get; set; }

        public int? SequenceOfCourse { get; set; }

        public string EducationalEnvironmentType { get; set; }

        public string AcademicSubjectDescriptor { get; set; }
    }
}
