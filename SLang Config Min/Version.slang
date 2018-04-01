unit Integer is end

abstract package Version[Major: Integer] is end
abstract package Version[Major, Minor: Integer] extend Version[Major] is end
abstract package Version[Major, Minor, Patch: Integer] extend Version[Major, Minor] is end
abstract package Version[Major, Minor, Patch: Integer, Tags: String] extend Version[Major, Minor, Patch] is end