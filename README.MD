# RT.XarkId - a 20-character, Base64Url-encoded, timestamp-injected UUID

In working on ideas to replace the aging GEDCOM standard for genealogy data transfer, I was interested in creating a universal standard for archiving records relating to genealogical and historical artifacts and facts.

I had a few goals for this ID scheme:

- Automatically globally unique without coordination
- Compatible with UUID/GUID database types
- Built-in UTC date/time stamp
- Compatible with GEDCOM identifiers (which are limited to 20 ASCII characters)
- Compatible with HTML DOM IDs
- Compatible with EAD IDs
- Compatible with XML Names
- Compatible with Windows and POSIX filenames
- No escaping required in HTML, XML, or URIs

The result is the **Extensible Archival (Xark) Identifier** (short name: **XID**). Characteristics of an XID:

- **120-bit** (15-byte) structure
- Can be represented as a standard **GUID/UUID**
- Can also be represented as a compact **20-character** ASCII string. In this form, it is fully compatible with GEDCOM IDs, XML/EAD IDs, DOM IDs, and URIs
- Contains a 48-bit, millisecond-resolution **timestamp**
- Contains 76 additional random bits for **global uniqueness**

This repository includes a reference implementation in .NET of the resulting data structure. It can encode and decode XARK IDs in both string and GUID forms.

## Representation as a GUID/UUID

GUIDs are 128 bits, so converting to and from XIDs requires that a total of 8 bits of the GUID that _are not_ part of the XarkId.

The first 4 of these bits are the GUID Version number (the most significant bits of byte 7). These are hard-coded as `4` (`0100b`).

The other 4 bits are the nybble where the GUID Variant is stored (the most-significant bits of byte 9). The first 1-3 of these bits are used for this variant. We use the variant `10` (RFC 4122/DCE 1.1 UUIDs). The other 2 bits are usually set randomly, but in the case of XIDs, they should always be set to `00`.

Note that converting bytes to and from GUIDs may require some swapping of bytes, especially on little-endian systems, since they may reorder bytes during the conversion.

## Representation as a compact string

XIDs are serialized and deserialized from 20-character `Base64Url` encoded strings (see [RFC4648](https://tools.ietf.org/rfc/rfc4648.txt), section 5 and table 2). It differs from standard base-64 encoding in two ways:

1. The alternate alphabet replaces `+` and `/` with `-` and `_`, allowing the strings to be filename and URI safe.
2. The trailing `=` is not needed (since the value falls on a 6-bit boundary) and is never included.

### Example

- GUID format: `123e4567-e89b-12d3-a456-426614174000`
- Base64Url format: `Ej5FZ-i7EtOkVkJmFBdAQA`

## Does this provide sufficient collision-preventing entropy?

The uniqueness is only as good as your pseudorandom number generator, so there are no absolute guarantees. However, out of our 120 bits of encoded information, 72 are randomized, and the other 48 are specific to each millisecond of time.

## Checksum UUIDs

Some GEDCOM implementations support UUID-based IDs (an extension to the standard), and some of those replace the last 32 bits with a CRC-32 _checksum_ of the data in the identified element. XIDs may be used for this purpose since these standards only modify bits that would otherwise be random in XIDs. This reduces the entropy somewhat, but still provides 40 bits of randomness for each millisecond, which is still plenty.

That said, care should be taken to only do this for IDs that are _intended_ to change over time, such as revision IDs, rather than as the primary ID for an entity.

## Caveats

GUID-encoded XIDs will sort in order of creation (within the resolution of the system clock(s) involved), but the string-encoded form will not. Databases storing XIDs should use the native `UUID` / `uniqueidentifier` datatype (or a 120-bit binary field), not the Base64Url string.

(Note that Microsoft SQL Server / Azure will _still_ not sort GUIDs by date, since it sorts in a different byte order. Care should be taken to avoid page fragmentation where these IDs are used as a clustered key.)

## Release History

| Date       | Version | Notes                                                         |
| ---------- | ------- | ------------------------------------------------------------- |
| 2017.07.11 | 1.0.0   | First version                                                 |
| 2021.01.03 | 2.0.0   | .NET 5, documentation rewrite                                 |
| 2023.01.04 | 2.0.1   | .NET 7                                                        |
| 2025.04.13 | 2.1.0   | .NET 9, .NET Standard 2.1, performance, benchmarks, new tests |

## License

Copyright 2017-2025 Richard S. Tallent, II

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
