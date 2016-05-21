# BnS Model Info
If you are looking for infos of BnS Unreal package file structure, I have nothing to tell you. If so, I point you to

https://sites.google.com/site/sippeyfunlabs/blade-and-soul-a-unreal-3-based-game-mod-section/upk-format
http://eliotvu.com/news/view/35/deserializing-unreal-packages-tables
http://www.gildor.org/smf/index.php?topic=2129.0#quickModForm

##What this is about
This is a tiny program for generating documents used by BnS model modifiers. Here is a sample of `BnS_ModelInfo_withName`:

```
60054_JinF.psk > 00013284
-----= Texture UPK: 00013282 =-
		col1.mat > 00013283 <洪门道服>
			60054_JinF_D.dds > 00013282
			60054_JinF_M.dds > 00013282
			60054_JinF_N.dds > 00013282
			60054_JinF_S.dds > 00013282
		col2.mat > 00013849
			60054_JinF_D_col2.dds > 00013282
			60054_JinF_M.dds > 00013282
			60054_JinF_N.dds > 00013282
			60054_JinF_S_col2.dds > 00013282
		col3.mat > 00019351 <变节者> <变节者(1天)> <变节者(3天)> <变节者(7天)> <变节者(30天)>
			60054_JinF_col3_D.dds > 00013282
			60054_JinF_col3_M.dds > 00013282
			60054_JinF_col3_N.dds > 00013282
			60054_JinF_col3_S.dds > 00013282
		col20.mat > 00021683
			60054_JinF_D.dds > 00013282
			60054_JinF_M.dds > 00013282
			60054_JinF_N.dds > 00013282
			60054_JinF_S.dds > 00013282
```

So. Top level objects are meshes. Then the texture packages it uses. Then the material constants refer to them. If it has a name(because game devs want gamers to play with it), or names, they are stacked there.

##How to use
明天再写。
