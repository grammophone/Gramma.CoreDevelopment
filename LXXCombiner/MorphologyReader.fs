namespace LXXCombiner

open System

type MorphologyReader(filename: string) =

  let fileReader = new System.IO.StreamReader(filename)
  
  let LineToVerse (line:string) : Verse = 

    let lineComponents = line.Split('\t')

    let bookCode = lineComponents.[0]
    
    { 
      BookNumber = int(bookCode.Substring(0, bookCode.Length - 1));
      ChapterNumber = int(lineComponents.[1]); 
      VerseNumber = int(lineComponents.[2]); 
      Words = lineComponents.[5].Split([|' '|], StringSplitOptions.RemoveEmptyEntries)
    }
  
  interface IVerseReader with
    member this.Dispose() = fileReader.Dispose()
    member this.Read() = seq {
      while not fileReader.EndOfStream do
        let verse = LineToVerse(fileReader.ReadLine())
        if verse.Words.Length > 0 
          then yield verse
    }