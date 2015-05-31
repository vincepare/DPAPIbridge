DPAPIbridge
---
DPAPIBridge is a command line tool designed to encrypt and decrypt data using [Windows Data Protection API](https://msdn.microsoft.com/en-us/library/ms995355.aspx) (DPAPI).

DPAPI is a windows built-in feature providing a ciphering service tied to the user windows account. DPAPI is only available through `CryptProtectData` and `CryptUnpotectData` functions, contained in `Crypt32.dll`, but there is no end user interface to it. This is what DPAPIbridge is for.

As a command line tool, DPAPIbridge is helpful to use DPAPI in a programming language that can't handle it natively (such as PHP). A typical use would be to store sensitive data (such as credentials) that you need at runtime from a scheduled task and you don't want to store those clear. Such sensitive data would be stored encrypted in a file, and then decrypted at runtime.

### Download ###
[Download dpapibridge.exe](https://github.com/finalclap/DPAPIbridge/releases/download/1.0.0/dpapibridge.exe)

##### Requirements
.NET Framework 2.0 or higher

Works on Windows XP, Windows Vista, Windows 7 & Windows 8.1.


### Examples
Encrypt raw :
```
dpapibridge --encrypt --input "foo bar" > encrypted.dat
echo foo bar | dpapibridge --encrypt > encrypted.dat
```

Encrypt using base64 encoding :
```
dpapibridge --encrypt --base64 --input Zm9vIGJhcg== > encrypted.dat
echo Zm9vIGJhcg== | dpapibridge --encrypt --base64 > encrypted.dat
```

Decrypt raw :
```
dpapibridge --decrypt < encrypted.dat
dpapibridge --decrypt --input "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAGMyvbyF [...]"
```

Decrypt using base64 encoding :
```
dpapibridge --decrypt --base64 < encrypted.dat
dpapibridge --decrypt --base64 --input "AQAAANCMnd8BFdERjHoAwE/Cl+sBAAAAGMyvbyF [...]"
```


### Usage ###
```bash
Usage: dpapibridge (--encrypt|--decrypt) [--base64] [--input=]
Options:
  -e, --encrypt              Encrypt input data
  -d, --decrypt              Decrypt input data
  -i, --input=VALUE          Get input data from this argument (rather than
                               stdin)
  -b, --base64               Encrypt mode : handle input as base64 encoded
                               data. Decrypt mode : output base64-encoded
                               result. Use it to avoid troubles when clear data
                               contains non ASCII bytes, like binary data.
  -o, --output=VALUE         Send output to file (instead of stdout)
  -?, -h, --help             Show this message and exit
```

Powered by [DPAPI class from obviex.com](http://www.obviex.com/samples/dpapi.aspx) to wrap Crypt32.dll and [Mono Options](https://github.com/mono/mono/tree/master/mcs/class/Mono.Options) (formerly known as NDesk Options) for command line parsing.
