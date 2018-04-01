abstract unit Any use const Integer, Real, Boolean, Character, String, Bit [Integer] is
	// Adding all constant objects from basic untis allows to use these constants without respective unit name prefix.


	/// Shallow equality tests
	routine = (that: ? as this ): Boolean is foreign
	final routine /= (that: ? as this ): Boolean => not ( this = that)
	routine = (that: as this): Boolean is foreign
	final routine /= (that: as this ): Boolean => not ( this = that)
	
	/// Deep equality tests
    	routine == (that: ? as this ): Boolean is foreign
	final routine /== (that: ? as this ): Boolean => not ( this == that)
    	routine == (that: as this): Boolean is foreign
	final routine /== (that: as this ): Boolean => not ( this == that)
	
	/// Assignment definition
	hidden routine := (that: ? as this ) is foreign
	hidden routine := (that: as this ) is foreign
	
	/// Utility
	routine toString: String is foreign
	routine sizeof: Integer is foreign
	
end // Any
