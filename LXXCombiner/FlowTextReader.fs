namespace LXXCombiner

open System
open System.Text.RegularExpressions

type Line =
  | Book of string
  | Chapter of string
  | VerseText of string
  | Other of string

type FlowTextReader(filename: string) =

  let fileReader = new System.IO.StreamReader(filename)

  let numbersRegex = new Regex("[0-9\\-]+", RegexOptions.Compiled ||| RegexOptions.Singleline)

  let GetLine(rawLine: string): Line =
    match rawLine with
      | _ when rawLine.Contains(":") -> Chapter rawLine
      | _ when numbersRegex.IsMatch(rawLine) -> VerseText rawLine
      | _ when rawLine.Length > 0 -> Book rawLine
      | _ -> Other rawLine

  let GetVerses(bookNumber: int, chapterNumber: int, rawLine: string): Verse seq = seq {
    let verseLines = numbersRegex.Split(rawLine)
    let numberMatches = numbersRegex.Matches(rawLine)

    for i = 0 to verseLines.Length - 1 do
      let verseLine = verseLines.[i]
      if i > 0 || verseLine.Length > 0 then
        yield { 
          BookNumber = bookNumber;
          ChapterNumber = chapterNumber;
          VerseNumber = if i = 0 then 1 else int(numberMatches.[i - 1].Value.Split([|'-'|]).[0])
          Words = 
            verseLine.Split([|' '; 'a'; 'b'; 'c'; 'd'; 'e'; 'f'; 'g'; 'h'; 'i'; 'j'; 'k'; 'l'; 'm'; 'n'; 'o'; 'p'; 'q'; 'r'; 's'; 't'|], 
              StringSplitOptions.RemoveEmptyEntries)
        }
  }
  
  interface IVerseReader with
    member this.Dispose() = fileReader.Dispose()
    
    member this.Read() = seq {
      let bookNumber = ref 0
      let chapterNumber = ref 0

      while not fileReader.EndOfStream do
        let rawLine = fileReader.ReadLine()
        let line = GetLine(rawLine)

        match line with
          | Book bookName -> bookNumber := !bookNumber + 1; chapterNumber := 0
          | Chapter chapterName -> chapterNumber := !chapterNumber + 1
          | VerseText rawVerseLine -> yield! GetVerses(!bookNumber, !chapterNumber, rawVerseLine)
          | Other rawLine -> raise(System.ApplicationException("Unexpected line: " + rawLine))

    }