# mixer-client-csharp
A C# client library for the Mixer streaming service

## What is this?
Unsatisfied with the current set of APIs that exist to interact with Mixer, I took some time to build and design an API set in C# to interact with the service. This API set can be used by anyone to build out any apps they wish to use and it’s very easy to setup.

## Current functionality
Most of the standard REST web API functions exist with some exceptions as I need to spent more time researching & testing them. I also plan to add API support for interactivity features when I get the chance.

## Feature roadmap
These are the current planned features to be developed in priority order:
- Finish full REST web API functionality with associated unit tests
- Develop interactive API functionality, unit tests, and sample app
- Develop full-scale sample app using both REST web APIs and interactive APIs
- Add ViewModels to all APIs to make ramp-up development work easier and put less focus on knowing the dev docs top-to-bottom

## How do I get started using it?
There are two sample apps created the help showcase some the setup needed and how to use the APIs. Additionally, there are a large serious of unit tests that go through all of the individual functionality that you can look at. I plan on creating more formal documentation soon.

## I found a bug, who do I contact?
Just head over to the https://github.com/SaviorXTanren/mixer-client-csharp/issues page and create a new issue.

## I have a new feature idea!
Submit feature requests at the https://github.com/SaviorXTanren/mixer-client-csharp/issues page or feel free to develop the feature yourself and submit a pull request at https://github.com/SaviorXTanren/mixer-client-csharp/pulls! I'm happy to assist anyone if they're interested in developing something as well.

## License
MIT License

Copyright (c) 2017 Matthew Olivo

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.