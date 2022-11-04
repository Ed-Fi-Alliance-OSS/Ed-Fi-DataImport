# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

import sys
 
for line in sys.stdin:
    if line.__contains__("DATAIMPORT_END_OF_FILE"):
        sys.exit(0)
    else:
        print(line)
