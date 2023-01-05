# RT.XarkId - a 20-character, Base-64 encoded, timestamp-injected UUID

In working on ideas to replace the aging GEDCOM standard for genealogy data transfer, I was interested in using globally-unique identifiers (specifically COMBs) for internal GEDCOM identifiers (which is limited to 20 characters). I also wanted to ensure that the same identifiers could be used as IDs in HTML DOM elements, EAD IDs, and XML names, and that they would never require escaping in HTML, XML, or URIs.

This is a reference implementation of that data structure. It can encode and decode them from strings or from GUIDs.

Characteristics of a XarkId:

- 120-bit (15-byte) structure
- Compact 20-character ASCII string form compatible with GEDCOM IDs, XML/EAD IDs, DOM IDs, and URIs without any encoding/escaping (always starts with a letter)
- Contains a 48-bit, millisecond-resolution timestamp
- Contains 76 additional random bits

## Converting to and from GUIDs

GUIDs are 128 bits, so converting to and from XarkIds requires that a total of 8 bits of the GUID that _are not_ part of the XarkId.

The first 4 of these bits are the GUID Version number (the most significant bits of byte 7) are the GUID/UUID version. These are hard-coded as `4` (`0100b`).

The other 4 bits are the nybble where the GUID Variant is stored (the most-significant bits of byte 9). The first 1-3 of these bits are used for this variant. We use the variant `10` (RFC 4122/DCE 1.1 UUIDs). The other 2 bits are usually set randomly, but in the case of XarkIds, they should always be set to `00`.

Note that converting bytes to and from GUIDs may require some swapping of bytes, since little-Endian systems may swap the bytes around during the conversion.

## Converting to and from strings

XarkIds are serialized and deserialized from 20-character `Base64Url` encoded strings (see [RFC4648](https://tools.ietf.org/rfc/rfc4648.txt), section 5 and table 2). It differs from standard base-64 encoding in two ways:

1. The alternate alphabet replaces `+` and `/` with `-` and `\_`, allowing the strings to be filename and URI safe.
2. The trailing `=` is not needed (since the value falls on a 6-bit boundary) and is never included.

## Does this provide sufficient collision-preventing entropy?

The uniqueness is only as good as your pseudorandom number generator, so there are no absolute guarantees. However, out of our 120 bits of encoded information, 72 are randomized, and the other 48 are specific to each millisecond of time.

## Checksum UUIDs

Some GEDCOM implementations support UUID-based IDs (an extension to the standard), and some of those replace the last 32 bits with a CRC-32 _checksum_ of the data in the identified element. XarkIds may be used for this purpose since these standards only modify bits that would otherwise be random in XarkIds. This reduces the entropy somewhat, but still provides 40 bits of randomness for each millisecond, which is still plenty.

That said, care should be taken to only do this for IDs that are _intended_ to change over time, such as revision IDs, rather than as the primary ID for an entity.

## Caveats

GUID-encoded XarkIds will sort in order of creation (within the resolution of the system clock(s) involved), but the string-encoded form will not. Databases storing XarkIds should use the native `UUID` / `uniqueidentifier` datatype (or a 120-bit binary field), not the Base64Url string.

(Note that Microsoft SQL Server / Azure will _still_ not sort GUIDs by date, since it sorts in a different byte order, so care should be taken to avoid page fragmentation where these IDs are used as a clustered key.)

## Release History

| Date       | Version | Notes                         |
| ---------- | ------- | ----------------------------- |
| 2017.07.11 | 1.0.0   | First version                 |
| 2021.01.03 | 2.0.0   | .NET 5, documentation rewrite |
| 2023.01.04 | 2.0.1   | .NET 7                        |

## License

Copyright 2017-2021 Richard S. Tallent, II

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
