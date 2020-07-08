# Id Generator

This project adds a source generator to your project that creates identity types. It's used for types that identifies some resource.
For example a user id or application id.

The source generator adds casting operators (explicit cast to, implicit cast from), equality checks, hash code, equals and `ToString()` (if it is not already provided).

It will get applied to a partial readonly struct that specifies `IEquality` on itself and has a single private readonly field with no constructors defined.
The struct currently must be declared in a namespace, i.e. you cannot declare them as inner types.

Example:

```csharp
public readonly partial struct UserId : IEquatable<UserId>
{
   private readonly Guid value;
}
```

This is all you have to write. Notice that this struct would not normally compile, so if it doesn't there's probably something wrong.
If you need more than a single field you can use a tuple on the value field. Note that the field doens't have to be named value.

You can add additional methods or static fields.

## License

This program is licensed under the MIT license. Please see license.txt for details.