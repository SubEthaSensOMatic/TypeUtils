using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System;
using TypeUtils.Services.Impl;

namespace TypeUtilsTest
{
    [TestClass]
    public class TypeConverterTest
    {
        [TestMethod]
        public void TestStandardConversion()
        {
            var germanCulture = CultureInfo.GetCultureInfo("de-DE");

            Assert.AreEqual(0.123, TypeConverter.Current.convert<double>("0,123", germanCulture));
            Assert.AreEqual(0.123, TypeConverter.Current.convert<double?>("0,123", germanCulture));
            Assert.AreEqual(1234.5, TypeConverter.Current.convert<double?>("1.234,5", germanCulture));
            Assert.IsNull(TypeConverter.Current.convert<double?>(null, germanCulture));

            Assert.AreEqual(123, TypeConverter.Current.convert<int>("123", germanCulture));
            Assert.AreEqual(123, TypeConverter.Current.convert<int?>("123", germanCulture));
            Assert.IsNull(TypeConverter.Current.convert<int?>(null, germanCulture));

            Assert.AreEqual(new DateTime(2017, 12, 31, 13, 59, 59), TypeConverter.Current.convert<DateTime>("31.12.2017 13:59:59", germanCulture));
        }

        [TestMethod]
        public void TestUriConversion()
        {
            Assert.AreEqual(new Uri(@"folder1\folder2\file.txt", UriKind.Relative), TypeConverter.Current.convert<Uri>(@"folder1\folder2\file.txt"));
            Assert.AreEqual(new Uri(@"c:\folder1\folder2\file.txt", UriKind.Absolute), TypeConverter.Current.convert<Uri>(@"c:\folder1\folder2\file.txt"));
        }

        [TestMethod]
        public void TestGuidConversion()
        {
            Assert.AreEqual(Guid.Parse("BE5B1F80-B878-44B4-8B61-E83FE2647C2F"), TypeConverter.Current.convert<Guid>("BE5B1F80-B878-44B4-8B61-E83FE2647C2F"));
        }

        public enum MyEnum
        {
            Arthur, Ford, Zaphod, Trillian, Marvin
        }

        [TestMethod]
        public void TestEnumConversion()
        {
            Assert.AreEqual(MyEnum.Marvin, TypeConverter.Current.convert<MyEnum>("Marvin"));
            Assert.AreEqual(MyEnum.Zaphod, TypeConverter.Current.convert<MyEnum>("Zaphod"));
        }
    }
}
