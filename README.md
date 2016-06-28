# TypeUtils
**TypeUtils** is a library containing threadsafe tools for type conversion and high performance data mapping. 

## Installation
You can download source and build project on your own or install package via nuget

```PowerShell
PM> Install-Package SimpleTypeUtils
```

## Mapping

**TypeUtils** provides an easy to use and performant mapping api. 

#### Mapping between properties
First example will show simple mapping between properties. To map objects you first have to create an instance of [Mapping&lt;T_Source, T_Target&gt;](Services/Mapping.cs) for two types. If property types doesn't match, then the mapping classes will try to convert them.

```c#
  
  // First type
  public class Person
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Street { get; set; }
    public string HouseNo { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public DateTime DayOfBirth { get; set; }
  }
  
  // Second type
  public class PersonDTO
  {
    public string FullName { get; set; }
    public string Street { get; set; }
    public int HouseNo { get; set; }
    public string ZipCode { get; set; }
    public string City { get; set; }
    public int Age { get; set; }
  }

  // Create mapping
  
  var mapping = (new Mapping<Person, PersonDTO> ())
    .map("LastName", "FullName") 
    .map("Street")
    .map("HouseNo") // will be converted automatically from string to int
    .map("ZipCode")
    .map("City");
  
```

After you have created the mapping rules, you have to instantiate an [ObjectMapper](Services/Impl/ObjectMapper.cs) or use the global instance and register mapping with mapper. After that you call one of the two mapping methods 

for simple mappings ```c# IEnumerable<T_Target> map<T_Source, T_Target>(IEnumerable<T_Source> source) ```

or for expensive mappings ```c# IList<T_Target> mapParallel<T_Source, T_Target>(IList<T_Source> source); ```

```c#

  // Use global instance to register mapping
  var mapper = ObjectMapper.Current;
  
  mapper.registerMapping(mapping);
  
  // Create persons
  var persons = new List<Person> () {
    new Person() {
      LastName = "Müller",
      Street = "Sesamstraße",
      HouseNo = "23",
      ZipCode = "12345",
      City = "Berlin"
    },
    new Person() {
      LastName = "Meyer",
      Street = "Hauptstraße",
      HouseNo = "42",
      ZipCode = "12345",
      City = "Berlin"
    });
  
  // Map persons to PersonDTO
  var result = mapper.map<Person, PersonDTO> (persons);

```

#### Complex mapping scenarios

For complexer mapping scenarios **TypeUtils** provides the possibility to set user defined functions for getting ans setting property values. 

```c#

  // Use global instance to register mapping
  var mapper = ObjectMapper.Current;
  
  // create complex mapping
  var mapping = (new Mapping<Person, PersonDTO> ())
    
    // Map first and last name to full name property
    .map((source, target) => source.First + " " + source.LastName  , "FullName") 
    
    // Map day of birth to age property (only for this example, to calculate real age use better approach)
    .map("DayOfBirth", (source, target, value) => {
      target.Age = DateTime.Now.Year - source.DayOfBirth.Year);
      });
  
  // register complex mapping
  mapper.registerMapping(mapping);
  
  // Create persons
  var persons = new List<Person> () {
    new Person() {
      FirstName = "Peter",
      LastName = "Müller",
      DayOfBirth = new DateTime(1980, 4, 21)
    },
    new Person() {
      LastName = "Knut",
      LastName = "Meyer",
      DayOfBirth = new DateTime(1959, 10, 13)
    });
  
  // Map persons to PersonDTO
  var result = mapper.map<Person, PersonDTO> (persons);

```



## Converting
**TypeUtils** converter class **TypeConverter** out of the box supports converting 
* All types implementing **IConvertible**
* All nullable types implementing **IConvertible**
* String <=> Enums
* String <=> Uri
* String <=> Guid
* Double => DateTime

```c#
// Convert string to double

// Use global TypeConverter.Current ...
var d1 = TypeUtils.TypeConverter.Current.convert("0.1234");

// ... or create your own instance
var conv = new TypeUtils.TypeConverter();

// convert with german culture
var d2 = conv.convert("1.120,1234", CultureInfo.GetCultureInfo("de-DE"));

```

## Customize conversion
You can customize **TypeConverter** and register new conversions or override existing conversions. To customize you have to register a Function with following signature

```c#
  [TargetType] customConvert([SourceType] sourceValze, Type sourceType, IFormatProvider format)
```

```c#
// Customize global converter TypeConverter.Current or your own instance

// Register conversion between DateTime and int (generic approach)
var d1 = TypeConverter.Current.registerCustomConverter<DateTime, int> ((dt, targetType, format) => {
  return dt.DayOfYear;
});

// do conversion
var days = TypeConverter.Current.convert<int>(new DateTime());

```
