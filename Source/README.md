##Dependency
To build, you need this library:
https://github.com/EliotVU/Unreal-Library

##Customizing Document Layout
This is the my layout for `BnS_ModelInfo_withName`. The structure is pretty clear to me. 
```
60116_JinF.psk > 00025374
-----= Texture UPK: 00025373 =-
		col1.mat > 00025372 <回忆（1天）> <回忆（3天）> <回忆（7天）> <回忆（30天）> <回忆>
			60116_JinF_col1_D.dds > 00025373
			60116_JinF_col1_E.dds > 00025373
			60116_JinF_col1_M.dds > 00025373
			60116_JinF_col1_N.dds > 00025373
			60116_JinF_col1_S.dds > 00025373
		col3.mat > 00032588 <青春（1天）> <青春（3天）> <青春（7天）> <青春（30天）> <青春>
			60116_JinF_col3_D.dds > 00025373
			60116_JinF_col3_M.dds > 00025373
			60116_JinF_col3_N.dds > 00025373
			60116_JinF_col3_S.dds > 00025373
		col2.mat > 00032602 <纯情（1天）> <纯情（3天）> <纯情（7天）> <纯情（30天）> <纯情>
			60116_JinF_col2_D.dds > 00025373
			60116_JinF_col2_M.dds > 00025373
			60116_JinF_col2_N.dds > 00025373
			60116_JinF_col2_S.dds > 00025373
		col4.mat > 00037910 <练习生（1天）> <练习生（3天）> <练习生（7天）> <练习生（30天）> <练习生>
			60116_JinF_col4_D.dds > 00025373
			60116_JinF_col4_M.dds > 00025373
			60116_JinF_col4_N.dds > 00025373
			60116_JinF_col4_S.dds > 00025373
			
```
To customize it, edit `fromUpkFile/PackageInfo_Analyzer.cs/UPackageInfo_Analyzer/Generate_ModelInfo()`.
Note, the `.dds`, `.psk`, `.mat` are just annotations for marking object type. If you have a problem with it, change it by editing the `const` strings in `UPackageInfo_Analyzer`.

Layout of `BnS_NameModel_Relation`:
```
General_Costum_Dress_1012
千魂衣
GB_Cash_Costume_Dress_0003_1day
千魂衣(1天)
GB_Cash_Costume_Dress_0003_3day
千魂衣(3天)
GB_Cash_Costume_Dress_0003_7day
千魂衣(7天)
GB_Cash_Costume_Dress_0003_30day
千魂衣(30天)
GB_Cash_Costume_Dress_0003_None
千魂衣
	00007401.ItemMove.S_ItemMove_Costume01Cue
	00007401.ItemDrag.S_ItemDrag_Costume01Cue
	00012232.60046_KunN
	00012058.60046_GonM
	00012306.60046_GonF
	00012196.60046_LynM
	00011386.60046_JinM
	00012176.60046_JinF
	00012231.col1
	00012057.col1
	00012305.col1
	00012195.col1
	00011385.col1
	00012175.col1
  
```
The `fromDatFile/DatInfo_Handler.cs/DatInfo_Generator/Generate_ObjHumanInfo()` controls it.
