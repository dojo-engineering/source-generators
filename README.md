# Source generator project

There are several generators in this project: AutoInterface and AutoException


## AutoInterface
The purpose of this generator is to save you from some boilerplate code. In many cases, we add an interface to a class to make it testable. However, every interface added like this pollutes a solution and adds some maintenance cost. The better approach is to automatically extract an interface with the source generator; the interface will always be up-to-date with the actual implementation.

### How to use:

1. Make your class partial.
2. Add AutoInterface attribute. It will automatically generate an interface and include all public methods.
3. Delete manually created interface.

This code:
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

Will generate code quivalent to:
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

## AutoException

Most of our exceptions are basic boilerplate and contain only standard constructors.
Instead, you can use AutoException attribute:

```
using DojoGenerator.Attributes;

namespace DojoGeneratorTest.Sample
{
    [AutoException]
    public partial class TestException
    {
    }
}
```

Which will generate code equivalent to this one:
```
using System;
using System.Runtime.Serialization;

namespace DojoGeneratorTest.Sample
{   
    public partial class TestException : Exception
    {
        public TestException()
        {
        }

        public TestException(string message) : base(message)
        {
        }

        public TestException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TestException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
```

