# BrowserVersions
An API for fetching the latest browser versions

## Motivation
Some services focus on supporting only a limited set of browsers (preferably) using up to date or at least ESR versions. However, updating the up to date browser version for every browser every half or year (or every release cycle, as it should be) is tedious.
This project aims to solve this by offering a unified API to request different versions of the (currently) three major browsers, Firefox, Chrome and Edge (Firefox comes first because it's awesome <3). Support for more browsers is planned in future versions.

## API
This is currently work in progress. I need to figure out why Azure App Service doesn't like my app and only displays the "Welcome Developer" screen, please bear with me.
Whenever this is done, you can find the Swagger doc at https://browserversions.azurewebsites.net/swagger/index.html
Requests should be easy to construct as the only thing you need to specify are the browsers you want (if none are specified, all are returned) and the channels you need (ESR, Nightly, you name it)
Both are enums so you can either use numbers or string based indexers (all hail the .NET modelbinder! Seriously, this is pretty damn cool)
TODO: Add some more API spec

## Future plans
There's actually quite a few things I have in mind! As most thing I do, this is an experiment that very few people except me might find practical but the more features the merrier, eh?
Anyway, here's a list off the top of my hat if you're really interested in those sort of things:
1. Add more browsers: This is mostly out of curiosity and based on what APIs I manage to dig up. Brave or Vivaldi are used frequently by some people so that might be worth looking into. Also did I mention Opera? (If they all follow the same version spec, because Chromium, this would be easy)
2. Add historical data: Would be cool to have some small database that has all the versions and their releases saved. Especially for when you want to support all browsers with a version released a year ago at most.
3. Implement saving versions: Currently the API makes a web request once for every browser (except IE because, well, it's IE). Automating this and updating the versions every day or so would save traffic and be more stable
4. Implement the same thing in some more languages. Yes that's right. This is an experiment after all. Since C# is my daily driver, I plan on doing the same thing over and over with different programming languages. Don't worry, the main branch will stay the way it is. That being said, I am currently mainly interested in Rust and Kotlin but also NodeJS to see how well they perform against what I'm used to. Also might look at Python and some functional thingy later on.

## License
Currently MIT, might change to EUPL later on. Still have to think about it