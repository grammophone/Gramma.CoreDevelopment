# Grammophone.CoreDevelopment
This is a Visual Studio 2015 solution for the development and testing of Ennoun, the part-of-speech and lemmatization system, using ancient Greek training sources. This repository contains submodules, so make sure you clone recursively.
The system revolves around central submodules [Grammophone.LanguageModel](https://github.com/grammophone/Grammophone.LanguageModel) and [Grammophone.EnnounInference](https://github.com/grammophone/Grammophone.EnnounInference).

Currently there are three main projects producing executables in the solution:
* `Grammophone.EnnounInference.Trainer`: A WPF application to train the system.
* `Grammophone.EnnounInference.Evaluator`: A WPF application to evaluate the system on fragments of text.
* `Grammophone.Lexica.Importer`: A command-line application to import lexica from their sources to a universal binary form.

There is also an F# project, the `LXXCombiner`, which combines sources of Septuagint Old Testament into one file, which is already included inside the `Grammophone.EnnounInference.Trainer/Training sets/LXX` folder. 

For more information about the sources used for training, see [`Grammophone.LanguageModel.Greek.TrainingSources`](https://github.com/grammophone/Grammophone.LanguageModel.Greek.TrainingSources).

Please note that the required file `Grammophone.EnnounInference.Trainer/Training sets/Perseus/greek.morph.xml` is too large to be included in the repository.
Please extract it from `Grammophone.EnnounInference.Trainer/Training sets/Perseus/greek.morph.zip`.
