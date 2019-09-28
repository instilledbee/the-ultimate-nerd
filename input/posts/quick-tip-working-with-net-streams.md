Title: Quick Tip When Working With C# Streams
Published: 9/28/2019
Tags: [.NET, C#]
---

This is more of a personal note that might be helpful for anyone stumped why their `Stream` objects are not copying contents properly. 

If you are doing any read operations on `Stream` instances, ensure that you are setting the `Position` properly to 0. This guarantees that the `Stream` will be read from its beginning and any calls to `Read` or `CopyTo` (and their `async` counterparts) will retrieve the entire stream from start to end.

A typical<sup>[*](#footnote)</sup> mistake would be to assume that `Streams` are instantiated with their position at the beginning. Whenever a `Stream` is created from existing contents, it is filled with data and thus the read position will often not be set to the beginning of the `Stream` as typically<sup>[*](#footnote)</sup> expected. 

Consider the following snippet:

``` 
// assume these variables have values
string fullPath;
byte[] data;

using (var newFile = new FileStream(fullPath, FileMode.Create))
using (var dataStream = new MemoryStream(data))
{
    return dataStream.CopyToAsync(newFile);
}
```

One would think that `dataStream` would copy its contents to `newFile` successfully. And that might have been the case&mdash; as this code compiles and produces no exceptions (as long as `newFile` was created without problems). But after inspecting the created file, it appears with a 0-byte filesize! So what happened then? 

As described earlier, it turns out that while `dataStream` would be created successfully (it exists, it has contents, the `Length` property can be compared to the length of the `data` byte array), its read position would be set to the end (i.e. right after the last byte copied from the array). So what to do then?

Make sure that the `MemoryStream` points to the beginning of the stream, to ensure the bytes written to it would be copied:

``` 
// assume these variables have values
string fullPath;
byte[] data;

using (var newFile = new FileStream(fullPath, FileMode.Create))
using (var dataStream = new MemoryStream(data))
{
    // reset stream position to ensure entire contents are copied
    dataStream.Position = 0;

    return dataStream.CopyToAsync(newFile);
}
```

After the added line, the `CopyToAsync()` call should now work as typically<sup>[*](#footnote)</sup> expected, creating a file with the correct contents.

*Note: some documentation might say to use `stream.Seek(0, SeekOrigin.Begin)` rather than `stream.Position = 0`. These produce identical behavior (i.e. setting the `Stream` position to its beginning) and can be safely interchanged. Setting the `Position` property is more useful for setting the absolute position, while `Seek()` is more useful for relative position changes (the first parameter is the position offset).*

<div id="footnote"><sub>* "typical" in this article refers to me, e.g. "a personal mistake" :)</sub></div>