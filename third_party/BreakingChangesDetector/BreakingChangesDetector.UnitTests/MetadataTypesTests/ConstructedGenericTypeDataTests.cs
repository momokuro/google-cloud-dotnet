﻿/*
    MIT License

    Copyright(c) 2014-2018 Infragistics, Inc.
    Copyright 2018 Google LLC

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using BreakingChangesDetector.MetadataItems;
using System;
using System.Collections.Generic;
using Xunit;

namespace BreakingChangesDetector.UnitTests.MetadataTypesTests
{
    public class ConstructedGenericTypeDataTests
    {
        [Fact]
        public void CustomConstructionTest()
        {
            var context = MetadataResolutionContext.CreateFromTypes(typeof(ConstructedGenericTypeDataTests));
            var type = context.GetTypeDefinitionData(typeof(List<>));
            var addMethod = type.GetMethod("Add");
            Assert.Equal("T", addMethod.Parameters[0].Type.Name);

            var constructedType = type.GetConstructedGenericTypeData(new[] { context.GetTypeData<TestTypeArgument>() });
            addMethod = constructedType.GetMethod("Add");
            Assert.Equal("TestTypeArgument", addMethod.Parameters[0].Type.Name);


            type = context.GetTypeDefinitionData(typeof(TestGenericTypeDefinition<>));
            constructedType = type.GetConstructedGenericTypeData(new[] { context.GetTypeData<TestTypeArgument>() });

            var constructor = (ConstructorData)constructedType.GetMember(".ctor");
            Assert.Equal("TestTypeArgument?", constructor.Parameters[0].Type.GetDisplayName(fullyQualify: false));
            var _event = (EventData)constructedType.GetMember("Event");
            Assert.Equal("EventHandler<TestTypeArgument>", _event.Type.GetDisplayName(fullyQualify: false));
            var field = (FieldData)constructedType.GetMember("Field");
            Assert.Equal("TestTypeArgument[]", field.Type.GetDisplayName(fullyQualify: false));
            var indexer = (IndexerData)constructedType.GetMember("Item");
            Assert.Equal("IList<TestTypeArgument>", indexer.Type.GetDisplayName(fullyQualify: false));
            Assert.Equal("TestTypeArgument[]", indexer.Parameters[0].Type.GetDisplayName(fullyQualify: false));
            var method = (MethodData)constructedType.GetMember("Method");
            Assert.Equal("TestTypeArgument?", method.Type.GetDisplayName(fullyQualify: false));
            Assert.Equal("TestTypeArgument", method.Parameters[0].Type.GetDisplayName(fullyQualify: false));
            Assert.Equal("U", method.Parameters[1].Type.GetDisplayName(fullyQualify: false));
            Assert.Equal("Dictionary<TestTypeArgument[], U[]>", method.Parameters[2].Type.GetDisplayName(fullyQualify: false));
            var _operator = (OperatorData)constructedType.GetMember("op_Addition");
            Assert.Equal("KeyValuePair<TestTypeArgument, object>", _operator.Type.GetDisplayName(fullyQualify: false));
            Assert.Equal("TestGenericTypeDefinition<TestTypeArgument>", _operator.Parameters[0].Type.GetDisplayName(fullyQualify: false));
            Assert.Equal("TestTypeArgument", _operator.Parameters[1].Type.GetDisplayName(fullyQualify: false));
            var property = (PropertyData)constructedType.GetMember("Property");
            Assert.Equal("IComparer<TestTypeArgument>", property.Type.GetDisplayName(fullyQualify: false));
            var nestedType = (ConstructedGenericTypeData)constructedType.GetMember("NestedType`1");
            Assert.Null(nestedType);

            type = context.GetTypeDefinitionData(typeof(TestGenericTypeDefinition<>.NestedType<>));
            constructedType = type.GetConstructedGenericTypeData(new[] { context.GetTypeData<TestTypeArgument>(), context.GetTypeData<double>() });
            Assert.Equal("NestedType<double>", constructedType.GetDisplayName(fullyQualify: false));
            Assert.Equal("BreakingChangesDetector.UnitTests.MetadataTypesTests.ConstructedGenericTypeDataTests.TestGenericTypeDefinition<TestTypeArgument>.NestedType<double>", constructedType.GetDisplayName());
            Assert.Equal("Tuple<int, TestTypeArgument[], double>", constructedType.BaseType.GetDisplayName(fullyQualify: false));

            type = context.GetTypeDefinitionData(typeof(TestGenericTypeDefinition<>.NestedType<>.FurtherNestedType<>));
            constructedType = type.GetConstructedGenericTypeData(new[] { context.GetTypeData<int>(), context.GetTypeData<TestTypeArgument>(), context.GetTypeData<double>() });
            Assert.Equal("FurtherNestedType<double>", constructedType.GetDisplayName(fullyQualify: false));
            Assert.Equal("BreakingChangesDetector.UnitTests.MetadataTypesTests.ConstructedGenericTypeDataTests.TestGenericTypeDefinition<int>.NestedType<TestTypeArgument>.FurtherNestedType<double>", constructedType.GetDisplayName());
            Assert.Equal("Dictionary<int, Tuple<TestTypeArgument, double>>", constructedType.BaseType.GetDisplayName(fullyQualify: false));
        }

        protected struct TestTypeArgument { }

        protected class TestGenericTypeDefinition<T> : Dictionary<int, T[]>,
            IEquatable<IDictionary<T, IList<T>>>
            where T : struct
        {
            public TestGenericTypeDefinition(ref T? x) { }

#pragma warning disable 0067
            public event EventHandler<T> Event;
#pragma warning restore 0067

            public T[] Field;

            public IList<T> this[T[] values]
            {
                set { }
            }

            public T? Method<U>(T x, U y, Dictionary<T[], U[]> z)
            {
                return null;
            }

            public static KeyValuePair<T, object> operator +(TestGenericTypeDefinition<T> a, T b)
            {
                return new KeyValuePair<T, object>();
            }

            public IComparer<T> Property
            {
                set { }
            }

            bool IEquatable<IDictionary<T, IList<T>>>.Equals(IDictionary<T, IList<T>> other)
            {
                throw new NotImplementedException();
            }


            public class NestedType<U> : Tuple<int, T[], U>
            {
                public NestedType(int x, T[] y, U z)
                    : base(x, y, z) { }

                public class FurtherNestedType<V> : Dictionary<T, Tuple<U, V>>
                {
                }
            }
        }
    }
}
