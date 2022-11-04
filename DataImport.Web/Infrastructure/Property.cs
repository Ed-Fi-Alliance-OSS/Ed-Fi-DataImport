// SPDX-License-Identifier: Apache-2.0
// Licensed to the Ed-Fi Alliance under one or more agreements.
// The Ed-Fi Alliance licenses this file to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICES files in the project root for more information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DataImport.Web.Infrastructure
{
    public static class Property
    {
        public static PropertyInfo From<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            return (PropertyInfo)GetMember(expression);
        }

        private static MemberInfo GetMember<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            var memberExpression = UnpackMemberExpression(expression); // x.Property

            return memberExpression.Member; // Property
        }

        private static MemberExpression UnpackMemberExpression<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
        {
            // The calling code may specify a lambda to a specific property:
            //      x => x.Property

            // However, depending on the type of that property, the incoming expression
            // object may arrive here in one of two forms:
            //
            //      x => (object)x.Property
            //
            //      x => x.Property

            // If the expression includes the cast, then it will be a UnaryExpression,
            // and we will need to unpack the operand of that expression in order to
            // access the original lambda's body.

            var body = expression.Body; // (object)x.Property OR x.Property

            if (body is UnaryExpression castToObject) // (object)x.Property
                body = castToObject.Operand; // x.Property

            return (MemberExpression)body; // x.Property
        }
    }
}