# Galaxy

## TODO

* Add CosmicMachine
* Add messages generator
* Add WAV generator
* Add WebPad - cleanup it's code
* Add alien_encode/alien_decode tools
* Publish to the Internet
* Publish to the dockerhub
* Write this readme
* Make link to icfpcontest2020.github.io in swagger docs
* Add descriptions for swagger methods (take from read-the-docs)
* Update read-the-docs




## How to build and run locally

1. Clone, build with docker and run on port `12345`:

   ```bash
   git clone git@github.com:icfpcontest2020/galaxy.git
   cd galaxy
   docker build -t planetwars .
   docker run --rm -p 12345:12345 planetwars
   ```

2. Open this url in browser: `http://localhost:12345` - you
   will see swagger docs page for PlanetWarsServer API
   
3. Try to send simple `Countdown` request (`[0]`, which is encoded as `1101000`):
   ```bash
   $ curl -d "1101000" -X POST "http://localhost:12345/aliens/send"
   ```
   
   You should see encoded response `[1,0]` (success, countdown is zero):
   ```bash
   11011000011101000
   ```    
   
3. So, if everything is fine, then you can use address `http://localhost:12345`
   as `serverUrl` for your Galaxy Pad implementation.  