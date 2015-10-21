# ![icon](https://raw.github.com/punker76/simple-music-player/master/icon/Gramaphone48x48.png) Simple Music Player

[![Gitter chat](https://badges.gitter.im/punker76/simple-music-player.png)](https://gitter.im/punker76/simple-music-player)  

[![Build status](https://img.shields.io/appveyor/ci/punker76/simple-music-player/master.svg?style=flat-square)](https://ci.appveyor.com/project/punker76/simple-music-player/branch/master)
[![Twitter](https://img.shields.io/badge/twitter-%40punker76-55acee.svg?style=flat-square)](https://twitter.com/punker76)

This is the next version of simple music player ([SimpleMP](http://jkarger.de/simple-music-player/))

**SimpleMP** or **Simple Music Player** is a simple and easy to use music player for free. It plays the most common music files (mp3, ogg, wma, wav) in a simple way.

Why another music player? When I decided to make my first player back in 2005, the Winamp 3 player was to buggy and very slow! I stopped developing in 2007.
I used my favorite programming language Delphi 5 and I learned a lot of  new stuff. It was a great time and a few people in the Delphi community honored my work :-)
Unfortunately, the interface of the FMOD sound library was no longer supported for Delphi and I decided to stop working on it.

After long time, now it's time to make a new one!

It's not perfect and many function not yet implemented.

***Use it with your own risk!***

## First simple functions

+ Plays **mp3**, **ogg**, **wma** and **wav** files (should be enough... or not?)
+ Drag&Drop your files or directories into the player (on the play list)
+ Drag&Drop files within the playlist
+ Add directories to the medialib (not yet finished)
+ Simple 10 band equalizer
+ Simple volume fade in and out (5 sec per default, can be changed in the settings file)
+ Shortcut keys if the player is on top and has the focus
	+ **[Space]** play or pause current file (if none is playing, the first selected file starts)
	+ **[Enter]** play selected files
	+ **[Del]** delete selected files from current playlist
	+ **[j]** play next file
	+ **[k]** play previuos file
	+ **[s]** shuffle mode on / off
	+ **[r]** repeat play list on / off
	+ **[m]** mute player on / off
	+ **[e]** show the equalizer
	+ **[l]** show the medialib
+ Flexible, responsive UI (different views for different window sizes)
+ You can link with the supported files (open with...)
+ Working as single instance, that means
	+ if you try start the player twice you get always the first started player
	+ if you link with supported media files, the player loads the new files and play the first file from the added files
+ Tooltip on playlist files

## Releases

- [v1.9.1](https://github.com/punker76/simple-music-player/releases/tag/v1.9.1)
- [v1.9.0.14](https://github.com/punker76/simple-music-player/releases/tag/v1.9.0.14)
- [v1.9.0.13](https://github.com/punker76/simple-music-player/releases/tag/v1.9.0.13)
- [v1.1.7.2](http://jkarger.de/simple-music-player/)

## Awesome libs that I use

- [MahApps.Metro](https://github.com/MahApps/MahApps.Metro)
- [MaterialDesignInXamlToolkit](https://github.com/ButchersBoy/MaterialDesignInXamlToolkit) Google Material Design in XAML & WPF, for C# & VB.Net from @ButchersBoy
- [gong-wpf-dragdrop](https://github.com/punker76/gong-wpf-dragdrop)
- [Font-Awesome-WPF](https://github.com/charri/Font-Awesome-WPF) FontAwesome controls for WPF from @charri
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- [NLog](https://github.com/NLog/NLog)
- [QuickIO.NET](http://quickio.azurewebsites.net/)
- [ReactiveUI](https://github.com/reactiveui/ReactiveUI)
- [Rx (Reactive Extensions)](http://rx.codeplex.com/)
- [Splat](https://github.com/paulcbetts/splat)
- [TinyIoC](https://github.com/grumpydev/TinyIoC)
- [xunit](https://github.com/xunit/xunit)

## Icons

Most Icons are taken (inspired) from (thx @Templarian)

- [Windows Icons](https://github.com/Templarian/WindowsIcons) -> [modernuiicons](http://modernuiicons.com)
- [Material Design](https://github.com/Templarian/MaterialDesign) -> [materialdesignicons.com](http://materialdesignicons.com/)
- [Metrize Icons](http://www.alessioatzeni.com/metrize-icons/) by [Alessio Atzeni](https://twitter.com/Bluxart) @Bluxart

## Screen shots (milestones)

![very early screenshot 20](https://raw.github.com/punker76/simple-music-player/master/screenshots/2015-10-20_23h51_14.png)  

![very early screenshot 19](https://raw.github.com/punker76/simple-music-player/master/screenshots/2015-10-20_23h50_59.png)  

![very early screenshot 18](https://raw.github.com/punker76/simple-music-player/master/screenshots/2015-10-20_23h50_51.png)  

![very early screenshot 17](https://raw.github.com/punker76/simple-music-player/master/screenshots/2015-07-19_22h18_51.png)  

![very early screenshot 16](https://raw.github.com/punker76/simple-music-player/master/screenshots/2015-07-19_22h19_10.png)  

![very early screenshot 15](https://raw.github.com/punker76/simple-music-player/master/screenshots/2014-11-02_12h11_31.png)  

![very early screenshot 14](https://raw.github.com/punker76/simple-music-player/master/screenshots/2014-07-14_22h42_19.png)  

![very early screenshot 13](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-11-26_20h58_04.png)  

![very early screenshot 12](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-11-26_20h58_17.png)  

![very early screenshot 11](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-11-10_23h42_10.png)  

![very early screenshot 10](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-11-10_23h42_24.png)  

![very early screenshot 09](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-09-16_09h40_08.png)  

![very early screenshot 08](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-09-16_09h40_02.png)  

![very early screenshot 07](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-02-07_22h49_52.png)  

![very early screenshot 06](https://raw.github.com/punker76/simple-music-player/master/screenshots/2013-01-05_23h26_18.png)  

![very early screenshot 05](https://raw.github.com/punker76/simple-music-player/master/screenshots/2012-12-19_21h55_18.png)  

![very early screenshot 04](https://raw.github.com/punker76/simple-music-player/master/screenshots/2012-12-19_21h54_53.png)  

![very early screenshot 03](https://raw.github.com/punker76/simple-music-player/master/screenshots/2012-12-19_14h48_05.png)  

![very early screenshot 02](https://raw.github.com/punker76/simple-music-player/master/screenshots/2012-12-16_17h14_57.png)  

![very early screenshot 01](https://raw.github.com/punker76/simple-music-player/master/screenshots/2012-12-13_23h14_25.png)  

## License and copyright

### Simple Music Player

Copyright (c) 2012-2015 Jan Karger

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

<http://opensource.org/licenses/MIT/>

### FMOD Ex SoundSystem

FMOD, FMOD Ex, FMOD Designer and FMOD Studio are 
Copyright © 2005-2015 Firelight Technologies Pty, Ltd.

GRANT OF LICENSE
----------------
THIS END USER LICENSE AGREEMENT GRANTS THE USER, THE RIGHT TO USE FMOD,
IN ITS LIBRARY AND TOOL FORM, IN THEIR OWN PRODUCTS, BE THEY FOR PERSONAL,
EDUCATIONAL OR COMMERCIAL USE.  
THE USER MUST ADHERE TO THE LICENSING MODEL PROVIDED BY FIRELIGHT 
TECHNOLOGIES, AND MUST APPLY FOR A LICENSE IF NECESSARY.  THE FOLLOWING
LICENSES ARE AVAILABLE.

FMOD NON-COMMERCIAL LICENSE
------------------------------------
IF YOUR PRODUCT IS NOT INTENDED FOR COMMERCIAL GAIN AND DOES NOT 
INCLUDE THE FMOD LIBRARY FOR RESALE, LICENSE OR OTHER COMMERCIAL 
DISTRIBUTION, THEN USE OF FMOD IS FREE OF CHARGE.  THERE ARE NO 
LICENSE FEES FOR NON-COMMERCIAL APPLICATIONS.
THE USER MAY USE THIS EULA AS EVIDENCE OF THEIR LICENSE WITHOUT 
CONTACTING FIRELIGHT TECHNOLOGIES.

CONDITIONS/LIMITATIONS:
- WHEN USING THIS LICENSE, THE FMOD LIBRARY CANNOT BE USED FOR 
  RESALE OR OTHER COMMERCIAL DISTRIBUTION 
- THIS LICENSE CANNOT BE USED FOR PRODUCTS WHICH DO NOT MAKE 
  PROFIT BUT ARE STILL COMMERCIALLY RELEASED 
- THIS LICENSE CANNOT BE USED FOR COMMERCIAL SERVICES, WHERE THE 
  EXECUTABLE CONTAINING FMOD IS NOT SOLD, BUT THE DATA IS.
- WHEN USING FMOD, A CREDIT LINE IS REQUIRED IN EITHER DOCUMENTATION, 
  OR 'ON SCREEN' FORMAT (IF POSSIBLE). IT SHOULD CONTAIN AT LEAST 
  THE WORDS "FMOD" (OR "FMOD STUDIO" IF APPLICABLE) AND 
  "FIRELIGHT TECHNOLOGIES."
  LOGOS ARE AVAILABLE FOR BOX OR MANUAL ART, BUT ARE NOT MANDATORY. 
  AN EXAMPLE CREDIT COULD BE:
  
  FMOD Sound System, copyright © Firelight Technologies Pty, Ltd., 1994-2015.
  OR
  FMOD Studio, copyright © Firelight Technologies Pty, Ltd., 1994-2015.
  OR 
  Audio Engine supplied by FMOD by Firelight Technologies.

  NOTE THIS IN ADVANCE, AS IT MUST BE DONE BEFORE SHIPPING YOUR 
  PRODUCT WITH FMOD.

FMOD FREE FOR INDIES LICENSE (FMOD STUDIO ONLY)
------------------------------------------------
INDIE DEVELOPERS ARE CONSIDERED BY OUR LICENSING MODEL, DEVELOPERS THAT DEVELOP
A TITLE FOR UNDER $100K USD (TYPICALLY CONSIDERED AN 'INDIE' TITLE) TOTAL 
BUDGET, MEANING YOUR TOTAL COSTS ARE LESS THAN $100K USD AT TIME OF SHIPPING, 
YOU CAN USE FMOD FOR FREE.

CONDITIONS/LIMITATIONS
- PLEASE WRITE TO SALES@FMOD.COM WITH THE NAME OF YOUR TITLE, RELEASE DATE 
  AND PLATFORMS SO WE CAN REGISTER YOU IN OUR SYSTEM.
- THERE IS NO RESTRICTION ON PLATFORM, ANY PLATFORM COMBINATION MAY BE USED.
- INCOME IS NOT RELEVANT TO THE BUDGET LEVEL, IT MUST BE EXPENSE RELATED.
- WHEN USING FMOD, A CREDIT LINE IS REQUIRED IN EITHER DOCUMENTATION, 
  OR 'ON SCREEN' FORMAT (IF POSSIBLE). IT SHOULD CONTAIN AT LEAST 
  THE WORDS FMOD STUDIO AND FIRELIGHT TECHNOLOGIES. 
  LOGOS ARE AVAILABLE FOR BOX OR MANUAL ART, BUT ARE NOT MANDATORY. 
  AN EXAMPLE CREDIT COULD BE:
  
  FMOD STUDIO, COPYRIGHT © FIRELIGHT TECHNOLOGIES PTY, LTD., 1994-2015.

COMMERCIAL USAGE (FMOD EX AND FMOD STUDIO)
------------------------------------------
IF THE PRODUCT THAT USES FMOD IS INTENDED TO GENERATE INCOME, VIA DIRECT SALES
OR INDIRECT REVENUE (SUCH AS ADVERTISING, DONATIONS, CONTRACT FEE) THEN THE 
DEVELOPER MUST APPLY TO FIRELIGHT TECHNOLOGIES FOR A COMMERCIAL LICENSE (UNLESS
THE USER QUALIFIES FOR AN FMOD STUDIO 'INDIE LICENSE').
TO APPLY FOR THIS LICENSE WRITE TO SALES@FMOD.COM WITH THE RELEVANT DETAILS.

REDISTRIBUTION LICENSE (FMOD EX AND FMOD STUDIO)
------------------------------------------------
IF THE USER WISHES TO REDISTRIBUTE FMOD AS PART OF AN ENGINE OR TOOL SOLUTION,
THE USER MUST APPLY TO FIRELIGHT TECHNOLOGIES TO BE GRANTED A 'REDISTRIBUTION
LICENSE'.  
TO APPLY FOR THIS LICENSE WRITE TO SALES@FMOD.COM WITH THE RELEVANT DETAILS.

WARRANTY AND LIMITATION OF LIABILITY
------------------------------------
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION
OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

FMOD Uses Ogg Vorbis codec.  BSD license.
-----------------------------------------
Copyright (c) 2002, Xiph.org Foundation

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:

- Redistributions of source code must retain the above copyright
notice, this list of conditions and the following disclaimer.

- Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.

- Neither the name of the Xiph.org Foundation nor the names of its
contributors may be used to endorse or promote products derived from
this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE ARE DISCLAIMED.  IN NO EVENT SHALL THE FOUNDATION
OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

For Android platform code.
--------------------------
Copyright (C) 2010 The Android Open Source Project
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
 * Redistributions of source code must retain the above copyright
   notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright
   notice, this list of conditions and the following disclaimer in
   the documentation and/or other materials provided with the
   distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
"AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS
OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED
AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF
SUCH DAMAGE.
