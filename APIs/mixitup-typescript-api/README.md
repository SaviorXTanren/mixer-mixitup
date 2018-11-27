# mixitup-typescript-api

A Node.js module that interfaces with a locally running Mix It Up Bot.  To use these features, the "Developer API" service needs to be enabled in the Services list.

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
mixitup.Chat.getAllUsersAsync().then((usersInChat) => {
  console.log(JSON.stringify(usersInChat));
});
```

### TypeScript
```typescript
import { Chat } from 'mixitup-typescript-api';
let usersInChat = await Chat.getAllUsersAsync();
console.log(JSON.stringify(usersInChat));
```
