[![NuGet](https://img.shields.io/nuget/v/Shtomper.svg)](https://www.nuget.org/packages/Shtomper)
[![License](https://img.shields.io/:license-BSD-orange.svg)](https://raw.githubusercontent.com/Cvazer/Shtomper/main/LICENSE)

This is a basic implementation of [STOMP](https://stomp.github.io/index.html) protocol which works over Websocket to transfer lightweight frames. 

This repository contains 3 major parts:
* Abstract layer (**Shtomper**)
* Concrete Websocket Implementation (**Shtomper-Client-Websocket**)
* JSON Converter implementation (**Shtomper-Converter-NewtonsoftJson**)

Shtompr is built upon a layer of abstraction so it would be easy to implement clients with different underlying transport mechanisms as well as new converters to support different formats of data serialization.

### How to use

```StompClientBuilder``` - is a part of Shtomper abstraction layer. A base point to start building a client. It's a builder that contains parameters specific to STOMP protocole.

```WebSocketStompClientFactoryBuilder``` - is a part Shtomper-Client-Websocket client implementation. It's a build that extends ```IStompClientFactoryBuilder``` interface provided by *Shtomper* abstraction and allows to configure implementation specific details (i. e. Websocket details in this case).

``Build(..)`` Method return an instance of ```IStompClientFactory``` specific to given concrete builder (i. e. ```WebSocketStompClientFactory``` for ```WebSocketStompClientFactoryBuilder```) which then can be used to create an instance of ```IStompClient``` which again is specific to builder/factory implementation. 

So, in short create shtomper builder => pass *specific* builder implementation to it's ```WithBuilder(..)``` method => build specific factory => create specific client.

#### Example usage

```c#
var waitHandle = new ManualResetEvent(false);

using var client = new StompClientBuilder()
    .SetMessageConverter(new NewtonsoftJsonMessageConverter())
    .SetUsername("guest")
    .SetPasscode("guest")
    .SetHeartbeatCapable(1000)
    .SetHeartbeatDesired(1000)
    .WithBuilder(new WebSocketStompClientFactoryBuilder())
    .SetPort(15674)
    .SetHost("localhost")
    .SetHostOverride("/") //overrides [host] header for CONNECT frame
    .SetPath("ws")
    .Build()
    .Create();

Action<Dictionary<string, string>> callback = Console.WriteLine;

client.Subscribe("/queue/testQueue", callback);

client.Send(
    "/queue/testQueue",
    new Dictionary<string, string> {
        { "firstName", "John" },
        { "lastName", "Doe" }, 
    }
);

waitHandle.WaitOne(TimeSpan.FromSeconds(5));
```

#### **Warning!!!**
Project is in very early stage. If for some reason you wish to use it - be warned.

### Notes

* All messages are sent asynchronously, from the queue.
* All messages are received asynchronously to the queue where they are being processed by different threads, so your callback will be called on a different thread (obviously)
