module LXXCombiner.Main

open System.IO
open LXXCombiner

let CombineTwoVerses (verse1, verse2) =
  {
    BookNumber = verse1.BookNumber;
    ChapterNumber = verse1.ChapterNumber
    VerseNumber = verse1.VerseNumber
    Words = 
      Array.zip verse1.Words verse2.Words
      |> Array.map(fun (word1, word2) -> word1 + " " + word2)
  }

let GetVerseTextOutput verse =
  sprintf "%d\t%d\t%d\t%s" 
    verse.BookNumber 
    verse.ChapterNumber 
    verse.VerseNumber 
    (Array.fold (fun accumulator word -> accumulator + word + " ") "" verse.Words)

let AreVersesCompatible (verse1, verse2) =
  verse1.BookNumber = verse2.BookNumber && verse1.ChapterNumber = verse2.ChapterNumber && 
    verse1.VerseNumber = verse2.VerseNumber && verse1.Words.Length = verse2.Words.Length

let AreChaptersEqual verse1 verse2 =
  verse1.ChapterNumber = verse2.ChapterNumber

try
  use morphologyReader = 
    new MorphologyReader(@"\Projects\dotNet\Solutions\Gramma\LXXCombiner\Sources\Μορφολογία.txt") :> IVerseReader
  
  use flowTextReader = 
    new FlowTextReader(@"\Projects\dotNet\Solutions\Gramma\LXXCombiner\Sources\Παλαιά Διαθήκη.txt") :> IVerseReader

  let morphologyVerses = morphologyReader.Read()
  let textVerses = flowTextReader.Read()

  let combinedVerses = 
    Seq.zip textVerses morphologyVerses
    |> Seq.filter AreVersesCompatible
    |> Seq.map CombineTwoVerses

  use writer = 
    new StreamWriter(
      @"\Projects\dotNet\Solutions\Gramma\LXXCombiner\Sources\Μορφολογία Παλαιᾶς Διαθήκης.txt",
      false,
      System.Text.Encoding.UTF8)

  Seq.iter (GetVerseTextOutput >> writer.WriteLine) combinedVerses

with
  | ex -> printfn "Exception type %s, message: %s" (ex.GetType().ToString()) ex.Message

