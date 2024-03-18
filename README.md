This is a basic implementation of [STOMP](https://stomp.github.io/index.html) protocol which works over Websocket to transfer lightweight frames. 

This repository contains 3 major parts:
* Abstract layer (**Shtomper**)
* Concrete Websocket Implementation (**Shtomper-Client-Websocket**)
* JSON Converter implementation (**Shtomper-Converter-NewtonsoftJson**)

Shtompr is built upon a layer of abstraction so it would be easy to implement clients with different underlying transport mechanisms as well as new converters to support different formats of data serialization.

#### **Warning!!!**
Project is in very early stage. If for some reason you wish to use it - be warned.
