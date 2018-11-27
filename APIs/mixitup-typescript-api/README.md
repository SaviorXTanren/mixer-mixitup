# mixitup-typescript-api

A Node.js module that interfaces with a locally running Mix It Up Bot.

## Installation 
```sh
npm install mixitup-typescript-api --save
yarn add mixitup-typescript-api
bower install mixitup-typescript-api --save
```

## Usage

### Javascript
```javascript
var mixitup = require('mixitup-typescript-api');
var boys = mixitup.getPlural('Boy');
```
```sh
Output should be 'Boys'
```

### TypeScript
```typescript
import { getPlural } from 'mixitup-typescript-api';
console.log(getPlural('Goose'))
```
```sh
Output should be 'Geese'
```

### AMD
```javascript
define(function(require,exports,module){
  var mixitup = require('mixitup-typescript-api');
});
```
