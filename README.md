# Source generator project

There are several generators in this project: AutoInterface and AutoException


## AutoInterface
The purpose of this generator is to save you from some boilerplate code. In many cases we add an interface to a class, just to make it testable.
Every interface added like this polutes a solution, and adds some maitanance cost. 

### How to use:
For example you have this class and interface:
```
// IFoo.cs
public class IFoo
{
  ReturnType Method1();
}

// Foo.cs
public class Foo : IFoo
{
    public ReturnType Method1()
    {
      // ...
    }
}
```

You can delete file with an interface, and change you class like this:
```
// Foo.cs
[AutoInterface]
public partial class Foo
{
    public ReturnType Method1()
    {
      // ...
    }
}
```

That will automatically generate an interface for all public methods.

## AutoException

TBD
