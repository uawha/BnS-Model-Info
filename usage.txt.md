Usage: bns_model_info [option] [option] [option] ...
    The order of your option list is irrelevant.
    Ah, this is not understandable. For quick start, jump to "Scenarios".

###Options:
```
    -m            Document option. If you want a model info doc.
                  When you flag this, you must specify -up=

   -mn            Document option. If you want a model info doc with names.
                  When you flag this, you must specify -up= and -dp=

    -n            Document option. If you want name-model relation doc.
                  When you flag this, you must specify -dp=

  -up=[Folder]    Input folder option. It is where the program looks for unreal packages.
                  The program will search in all subfolders.

  -dp=[Folder]    Input folder option. It is where the program looks for
                  datafile_230.xml and datafile_103.xml.
                  They are decompressed files of datafile.bin,
                  which is a decompressed file of xml.dat, you get them by bnsdat.
                  The program will search in all subfolders.

   -o=[Folder]    Output folder option. It is where the program write the generated file.
                  If not specified, it will use the current folder.
```
###Scenarios:

    1.1           You have a working bnsdat.exe, you've already had the datafile_230.xml and
                  datafile_103.xml, and you want to generate model info with names.
                  Suppose the folder contains your xml files is D:\datafile.bin.files,
                  and your game package folder is D:\b and s\contents, then you may write
                  `bns_model_info -mn "-up=D:\b and s\contents" -dp=D:\datafile.bin.files`
		
    1.2           You may flag multiple doc options. Don't worry about program performance.
	
    2             The bnsdat.exe is not working or you don't have one. In this case, the only working
                  doc option is `-m` since the game devs put names data in the `xml.dat`.
