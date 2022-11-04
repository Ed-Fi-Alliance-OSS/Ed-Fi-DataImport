# SPDX-License-Identifier: Apache-2.0
# Licensed to the Ed-Fi Alliance under one or more agreements.
# The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
# See the LICENSE and NOTICES files in the project root for more information.

import sys

print("Start with an error", file=sys.stderr)
print("Regular output")
print("Here's an in-between error", file=sys.stderr)
print("More regular output")
print("This is the last error printed", file=sys.stderr)
print("Output after error")
