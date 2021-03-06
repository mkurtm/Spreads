﻿using NUnit.Framework;
using Spreads.DataTypes;
using System;
using System.Runtime.Serialization;

namespace Spreads.Tests.DataTypes
{
    [TestFixture]
    public class TypeFlattenerTests
    {
        [Test]
        public void CouldFlattenStructWithDataMembers()
        {
            var tick = new Tick(DateTime.UtcNow.Date, new Price(123.45), 4242);
            var flattener = new TypeFlattenner(tick.GetType());
            object[] values = null;

            flattener.Flatten(tick, ref values);

            Console.WriteLine($"Columns: {flattener.Columns[0]} - {flattener.Columns[1]} - {flattener.Columns[2]}");

            // NB this differs from DateTimeUtc field
            Assert.AreEqual("DateTime", flattener.Columns[0]);
            Assert.AreEqual("Price", flattener.Columns[1]);
            Assert.AreEqual("Volume", flattener.Columns[2]);

            Assert.AreEqual(DateTime.UtcNow.Date, values[0]);
            Assert.AreEqual(new Price(123.45), values[1]);
            Assert.AreEqual(4242, values[2]);
        }

        [Test]
        public void CouldFlattenScalar()
        {
            var price = new Price(123.45);
            var flattener = new TypeFlattenner(price.GetType());
            object[] values = null;

            flattener.Flatten(price, ref values);

            Console.WriteLine($"Columns: {flattener.Columns[0]} ");

            Assert.AreEqual("Price", flattener.Columns[0]);
            Assert.AreEqual(new Price(123.45), values[0]);
        }

        [Test]
        public void CouldFlattenDouble()
        {
            var dbl = 123.45;
            var flattener = new TypeFlattenner(dbl.GetType());
            object[] values = null;

            flattener.Flatten(dbl, ref values);

            Console.WriteLine($"Columns: {flattener.Columns[0]} ");

            Assert.AreEqual("Double", flattener.Columns[0]);
            Assert.AreEqual(123.45, values[0]);
        }

        public class TestType
        {
            public int Number { get; set; }
            public string Text { get; set; }
            public Price Price { get; set; }
        }

        public class TestTypeWithPartialOrder
        {
            [DataMember]
            public int Number { get; set; }

            [DataMember(Order = 1)]
            public string Text { get; set; }

            [DataMember]
            public Price Price { get; set; }
        }

        [Test]
        public void CouldFlattenCustomType()
        {
            var value = new TestType { Number = 42, Text = "foo", Price = new Price(123.45) };
            var flattener = new TypeFlattenner(value.GetType());
            object[] values = null;

            flattener.Flatten(value, ref values);

            Console.WriteLine($"Columns: {flattener.Columns[0]} - {flattener.Columns[1]} - {flattener.Columns[2]}");

            Assert.AreEqual("Number", flattener.Columns[0]);
            Assert.AreEqual("Price", flattener.Columns[1]);
            Assert.AreEqual("Text", flattener.Columns[2]);

            Assert.AreEqual(42, values[0]);
            Assert.AreEqual(new Price(123.45), values[1]);
            Assert.AreEqual("foo", values[2]);
        }

        [Test]
        public void CouldFlattenCustomTypeWithPartialOrder()
        {
            var value = new TestTypeWithPartialOrder { Number = 42, Text = "foo", Price = new Price(123.45) };
            var flattener = new TypeFlattenner(value.GetType());
            object[] values = null;

            flattener.Flatten(value, ref values);

            Console.WriteLine($"Columns: {flattener.Columns[0]} - {flattener.Columns[1]} - {flattener.Columns[2]}");

            Assert.AreEqual("Text", flattener.Columns[0]);
            Assert.AreEqual("Number", flattener.Columns[1]);
            Assert.AreEqual("Price", flattener.Columns[2]);

            Assert.AreEqual("foo", values[0]);
            Assert.AreEqual(42, values[1]);
            Assert.AreEqual(new Price(123.45), values[2]);
        }
    }
}