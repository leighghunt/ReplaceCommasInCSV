ReplaceCommasInCSV
==================

Replace commas in large CSV files, whilst preserving commas within string field values.

By default, will remove double quotes in output, and replace commas with pipe
characters.

Should handle a files with column widths of a few hundred to a thousand characters at a rate of around 1,000,000 lines per minute, or roughly 1 GB/minute.

Work could quite probably be carried out more efficiently using regular expressions, but they just make my head hurt too much.

Usage:
------
````
ReplaceCommasInCSV filename [/R replacement string] [/O output filename] /Q
filename       Input filename
/R             Replacement string - commas not within quoted strings will be replaced with this string
               Optional - if omitted, a pipe ('|') is used
/O             Output filename - optional, if omitted, original file will be overwritten
/Q             Preserve double quotes, if specified, all double quotes will be left in output - default is to strip double quotes from output
````

Example:
--------

````ReplaceCommasInCSV.exe /r "|"  d:nz-primary-land-parcels.txt /o c:\users\lhunt\out.txt````

would take the following content in the file ````d:nz-primary-land-parcels.txt````:

````
A,B,C
123,456,789
123,"456,789",012
````

and output it to the file ````c:\users\lhunt\out.txt```` as:
````
A|B|C
123|456|789
123|456,789|012
````