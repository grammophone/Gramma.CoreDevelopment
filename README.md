# Gramma.CoreDevelopment
This is a Visual Studio 2015 solution for the development and testing of the part-of-speech and lemmatization system using ancient Greek training sources. This repository contains submodules, so make sure you clone recursively.
The system revolves around central submodules [Gramma.LanguageModel](https://github.com/grammophone/Gramma.LanguageModel) and [Gramma.Inference](https://github.com/grammophone/Gramma.Inference).

Currently there are three main projects producing executables in the solution:
* `Gramma.Inference.Trainer`: A WPF application to train the system.
* `Gramma.Inference.Evaluator`: A WPF application to evaluate the system on fragments of text.
* `Gramma.Lexica.Importer`: A command-line application to import lexica from their sources to a universal binary form.

There is also an F# project, the `LXXCombiner`, which combines sources of Septuagint Old Testament into one file, which is already included inside the `Gramma.Inference.Trainer/Training sets/LXX` folder. 

For more information about the sources used for training, see [`Gramma.LanguageModel.Greek.TrainingSources`](https://github.com/grammophone/Gramma.LanguageModel.Greek.TrainingSources).

Please note that the required file `Gramma.Inference.Trainer/Training sets/Perseus/greek.morph.xml` is too large to be included in the repository.
Please extract it from `Gramma.Inference.Trainer/Training sets/Perseus/greek.morph.zip`.
