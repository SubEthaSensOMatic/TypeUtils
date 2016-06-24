# TypeUtils
**TypeUtils** is library containing tools for type conversion and high performance data mapping. 

## Installation
You can download source and build project on your own or install package via nuget

```PowerShell
PM> Install-Package SimpleTypeUtils
```

## Converting types
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

## Mapping objects

## Create mapping definition

## Create property mapper

