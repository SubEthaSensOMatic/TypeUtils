using Microsoft.VisualStudio.TestTools.UnitTesting;
using TypeUtils.Extensions;
using System.Text;

namespace TypeUtilsTest
{
    [TestClass]
    public class TypeExtensionsTest
    {
        [TestMethod]
        public void TestIntegerType()
        {
            Assert.IsTrue(typeof(byte).IsIntegerType(), "IsIntegerType for byte is false!");
            Assert.IsTrue(typeof(byte?).IsIntegerType(), "IsIntegerType for byte? is false!");
            Assert.IsTrue(typeof(sbyte).IsIntegerType(), "IsIntegerType for sbyte is false!");
            Assert.IsTrue(typeof(sbyte?).IsIntegerType(), "IsIntegerType for sbyte? is false!");
            Assert.IsTrue(typeof(short).IsIntegerType(), "IsIntegerType for short is false!");
            Assert.IsTrue(typeof(short?).IsIntegerType(), "IsIntegerType for short? is false!");
            Assert.IsTrue(typeof(ushort).IsIntegerType(), "IsIntegerType for ushort is false!");
            Assert.IsTrue(typeof(ushort?).IsIntegerType(), "IsIntegerType for ushort? is false!");
            Assert.IsTrue(typeof(int).IsIntegerType(), "IsIntegerType for int is false!");
            Assert.IsTrue(typeof(int?).IsIntegerType(), "IsIntegerType for int? is false!");
            Assert.IsTrue(typeof(uint).IsIntegerType(), "IsIntegerType for uint is false!");
            Assert.IsTrue(typeof(uint?).IsIntegerType(), "IsIntegerType for uint? is false!");
            Assert.IsTrue(typeof(long).IsIntegerType(), "IsIntegerType for long is false!");
            Assert.IsTrue(typeof(long?).IsIntegerType(), "IsIntegerType for long? is false!");
            Assert.IsTrue(typeof(ulong).IsIntegerType(), "IsIntegerType for ulong is false!");
            Assert.IsTrue(typeof(ulong?).IsIntegerType(), "IsIntegerType for ulong? is false!");

            Assert.IsFalse(typeof(double).IsIntegerType(), "IsIntegerType for double is true!");
            Assert.IsFalse(typeof(string).IsIntegerType(), "IsIntegerType for string is true!");
        }

        [TestMethod]
        public void TestNumericType()
        {
            Assert.IsTrue(typeof(float).IsNumericType(), "IsNumericType for float is false!");
            Assert.IsTrue(typeof(float?).IsNumericType(), "IsNumericType for float? is false!");
            Assert.IsTrue(typeof(double).IsNumericType(), "IsNumericType for double is false!");
            Assert.IsTrue(typeof(double?).IsNumericType(), "IsNumericType for double? is false!");
            Assert.IsTrue(typeof(decimal).IsNumericType(), "IsNumericType for decimal is false!");
            Assert.IsTrue(typeof(decimal?).IsNumericType(), "IsNumericType for decimal? is false!");

            Assert.IsFalse(typeof(byte).IsNumericType(), "IsNumericType for byte is true!");
            Assert.IsFalse(typeof(string).IsNumericType(), "IsNumericType for string is true!");
        }

        [TestMethod]
        public void TestTextType()
        {
            Assert.IsTrue(typeof(string).IsTextType(), "IsTextType for string is false!");
            Assert.IsTrue(typeof(char).IsTextType(), "IsTextType for char is false!");
            Assert.IsTrue(typeof(char?).IsTextType(), "IsTextType for char? is false!");
            Assert.IsTrue(typeof(StringBuilder).IsTextType(), "IsTextType for StringBuilder is false!");

            Assert.IsFalse(typeof(byte).IsTextType(), "IsTextType for byte is true!");
            Assert.IsFalse(typeof(double).IsTextType(), "IsTextType for double is true!");
        }

        [TestMethod]
        public void GetDefaultValueTest()
        {
            Assert.IsNull(typeof(string).GetDefaultValue(), "Default value of string is not null!");
            Assert.AreEqual(0.0, typeof(double).GetDefaultValue(), "Default value of double is not 0.0!");
            Assert.AreEqual(0, typeof(int).GetDefaultValue(), "Default value of int is not 0!");
            Assert.IsNull(typeof(long?).GetDefaultValue(), "Default value of long? is not null!");
        }
    }
}
