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
/U             Handle unmatched quotes. If a newline is encountered inside a string, the
               newline will be replaced by '\\n'. Default is to not handle, warn user and exit

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

Preserving double quotes
------------------------

````ReplaceCommasInCSV.exe /r "|"  d:nz-primary-land-parcels.txt /o c:\users\lhunt\out.txt /q````

would take:
````
A,B,C
123,456,789
123,"456,789",012
````

and produce:
````
A|B|C
123|456|789
123|"456,789"|012
````

Handling newlines within quoted strings
---------------------------------------

If a string field value contained within double quotes contains a newline, by default this will report an error.

````ReplaceCommasInCSV.exe /r "|"  d:nz-primary-land-parcels.txt /o c:\users\lhunt\out.txt```` would take:
````
A,B,C
123,456,789
123,"456,
789",012
````
and produce:

````
c:\dev\ReplaceCommasInCSV\ReplaceCommasInCSV\bin\Release>ReplaceCommasInCSV.exe /r "|"  d:nz-primary-land-parcels.txt  /o c:\users\lhunt\out.txt
Opening file d:nz-primary-land-parcels.txt...
Writing to c:\users\lhunt\out.txt...

ERROR - found unmatched double quote in line 3 - EXITING!

Try running with /U switch to concatenate string field values that span multiple lines.

Failing line:

123,"456,
789",012
````

Whilst ````ReplaceCommasInCSV.exe /r "|"  d:nz-primary-land-parcels.txt /o c:\users\lhunt\out.txt /u````

would produce:
````
A|B|C
123|456|789
123|456,\n789|012
````