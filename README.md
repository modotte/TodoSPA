# TodoSPA

A todo single-page application with F# and Elmish (Elm architecture).


## Live Demo

[Demo Page](https://modotte.github.io/TodoSPA/)

## Building & Testing
### Requirements

* [dotnet SDK](https://dotnet.microsoft.com/download) 5.0 or higher
* [node.js](https://nodejs.org) 10.0.0 or higher


### Editor

To write and edit your code, you can use either VS Code + [Ionide](http://ionide.io/), Emacs with [fsharp-mode](https://github.com/fsharp/emacs-fsharp-mode), [Rider](https://www.jetbrains.com/rider/) or Visual Studio.


### Development

Before doing anything, start with installing npm dependencies using `npm install`.

Then to start development mode with hot module reloading, run:
```bash
npm start
```
This will start the development server after compiling the project, once it is finished, navigate to http://localhost:5080 to view the application .

To build the application and make ready for production:
```
npm run build
```
This command builds the application and puts the generated files into the `deploy` directory (can be overwritten in webpack.config.js).

#### Tests

The template includes a test project that ready to go which you can either run in the browser in watch mode or run in the console using node.js and mocha. To run the tests in watch mode:
```
npm run test:live
```
This command starts a development server for the test application and makes it available at http://localhost:8085.

To run the tests using the command line and of course in your CI server, you have to use the mocha test runner which doesn't use the browser but instead runs the code using node.js:
```
npm test
```

## Contributing

Please read [CONTRIBUTING.md](https://github.com/modotte/TodoSPA/blob/main/CONTRIBUTING.md) first.

Thank you for contributing! :smile:
## License

This software is released under the MIT license. For more details, please see LICENSE file.
