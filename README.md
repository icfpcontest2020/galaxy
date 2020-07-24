# Galaxy

## TODO

* Cleanup GalaxyPad code
* Publish to the Internet
* Publish to the dockerhub
* Write this readme
* Make link to icfpcontest2020.github.io in swagger docs
* Add descriptions for swagger methods (take from read-the-docs)
* Update read-the-docs

## How to change message images

1. Change source in src/CosmicMachine/message.txt
2. Run `render-messages.sh` (dotnet core should be installed)
3. See result in /message directory
 
## How to change galaxy.txt

1. Edit source in src/CosmicMachine/CSharpGalaxy
2. Run `compile-galaxy.sh` (dotnet core should be installed)
3. See result in /galaxy directory

## How to build and run Alien server locally

1. Clone, build with docker and run on port `12345`:

   ```bash
   $ git clone git@github.com:icfpcontest2020/galaxy.git
   $ cd galaxy
   $ docker build -t galaxy .
   $ docker run --rm -p 12345:12345 galaxy
   ```

2. Open this url in browser: `http://localhost:12345/swagger` - you
   will see swagger docs page for Galaxy API
   
3. Try to send simple `Countdown` request (`[0]`, which is encoded as `1101000`):
   ```bash
   $ curl -d "1101000" -X POST "http://localhost:12345/aliens/send"
   ```
   
   You should see encoded response `[1,0]` (success, countdown is zero):
   ```bash
   11011000011101000
   ```    
   
4. So, if everything is fine, then you can use address `http://localhost:12345`
   as `serverUrl` for your Galaxy Pad implementation.
   
## How to encode/decode Alien strings

Use provided bash-scripts `alien_encode.sh` and `alien_decode.sh` (dotnet core should be installed).

You can encode/decode strings from STDIN or from command line arguments:
```bash
$ ./alien_encode.sh "[0, 1]"
11010110110000100

$ ./alien_decode.sh 11010110110000100
[0, 1]

$ echo "[0, 1]" | ./alien_encode.sh
11010110110000100

$ echo 11010110110000100 | ./alien_decode.sh
[0, 1]
```

You also can pipe this encoding/decoding with sending requests to Alien server:

```bash
$ ./alien_encode.sh "[1,0]" | curl -X POST -s -d @- "http://localhost:12345/aliens/send" | ./alien_decode.sh
[1, [[0, 5939854065736244037], [1, 7463090120749941785]]]

$ ./alien_encode.sh "[1,0]" | http "http://localhost:12345/aliens/send" | ./alien_decode.sh
[1, [[0, 5939854065736244037], [1, 7463090120749941785]]]
```