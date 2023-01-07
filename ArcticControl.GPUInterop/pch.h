// pch.h: Dies ist eine vorkompilierte Headerdatei.
// Die unten aufgeführten Dateien werden nur einmal kompiliert, um die Buildleistung für zukünftige Builds zu verbessern.
// Dies wirkt sich auch auf die IntelliSense-Leistung aus, Codevervollständigung und viele Features zum Durchsuchen von Code eingeschlossen.
// Die hier aufgeführten Dateien werden jedoch ALLE neu kompiliert, wenn mindestens eine davon zwischen den Builds aktualisiert wird.
// Fügen Sie hier keine Dateien hinzu, die häufig aktualisiert werden sollen, da sich so der Leistungsvorteil ins Gegenteil verkehrt.

#ifndef PCH_H
#define PCH_H

#define _CRTDBG_MAP_ALLOC
#define CTL_APIEXPORT

// add all header which should be precompile here
#include <windows.h>
#include <crtdbg.h>
#include <iostream>
#include "igcl_api.h"

#define CTL_FREE_MEM(ptr)	\
	if (nullptr != ptr)		\
	{						\
		free(ptr);			\
		ptr = nullptr;		    \
	}						\

#endif //PCH_H
