namespace LXXCombiner

type Verse = { BookNumber: int; ChapterNumber: int; VerseNumber: int; Words: string [] }

type IVerseReader = 
  inherit System.IDisposable
  abstract Read: unit -> seq<Verse>
      

