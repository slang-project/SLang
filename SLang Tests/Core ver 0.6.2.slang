// Core units to represent basic SLang types  (WIP - not all features are fully implemented!!! )
// Version 0.6.2 June 6th 2017

val unit Bit
//    const is
//       0b0, 0b1
//    end
// 	routine & alias and (other: as this): as this => if this = 0b0 then 0b0 elsif other = 0b0 then 0b0 else 0b1;
//  
// 	routine | alias or (other: as this): as this => if this = 0b1 then 0b1 elsif other = 0b1 then 0b1 else 0b0;
//
//	routine ^ alias xor (other: as this): as this => if this = other then 0b0 else 0b1;
//		
//	routine ~ alias not : as this => if this = 0b0 then	0b1 else 0b0;
//		
//	routine + (other: as this): as this is
//		/// Definition of addition
//		if this = 0b0 then
//			return other
//		elsif other = 0b1 then
//			return 0b1
//		else
//			raise "Bit overflow"
//		end
//	end // +
	routine - (other: as this): as this is // abstract
		/// Definition of subtraction 
		if this = other then
			return 0b0
		elsif this = 0b1 then
			return 0b1
		else
			raise "Bit underflow"
		end
	end // -
end

val unit Bit [N: Integer] is
	const is
	    // 0b(N)0 .. 0b(N)1
	end
	hidden data: val Array [0..N-1, Bit]; // Bit field ...
	:= (other: Integer) is
	/// Set N low bits from Integer other into this
	// WIP!!!
		raise "Not implemented!!!"
		// WIP What if number of bits in Integer is less than N ?????
		
		while pos in 0.. N - 1 loop
			this (pos) := other.data (pos)
		end
	end
	override routine sizeof: Integer is
		// return sizeof in bytes !!!! N is a number of bits !!!
		return N / Platform.BitsInByteCount + if N \ Platform.BitsInByteCount = 0 then 0 else 1
	end
	routine & alias and (other: as this): as this is
		result is this
		while pos in 0 .. N - 1 loop
		    result.toggle (pos)
			result (pos) := if this(pos) = 0 or else other(pos) = 0 then 0 else 1
		end
		return result
	end
	routine | alias or (other: as this): as this is
		result is this
		while pos in 0 .. N - 1 loop
			result (pos) := if this(pos) = 1 or else other (pos) = 1 then 1 else 0
		end
		return result
	end
	routine ^ alias xor (other: as this): as this is
		result is this
		while pos in 0 .. N - 1 loop
			result (pos) := if this (pos) = other(pos) then 0 else 1
		end
		return result
	end
	routine ~ alias not : as this is
		result is this
		while pos in 0 .. N - 1 loop
			result.toggle (pos)
		end
		return result
	end
	routine () (pos: Integer; value: Boolean)
	require
		pos in 0 .. N - 1 /// Valid bit position
	is	
		this (pos) := if value then 1 else 0
	end
	routine () (pos: Integer; value: Integer)
	/// Set bit as position 'pos' to value 'value'
	require
		pos in 0 .. N - 1 /// Valid bit position
		value in 0..1 /// Valid value
	is
		if value = 0 then
			data (pos) := 0b0
		else
			data (pos) := 0b1
		end
	end
	routine () (pos: Integer): Boolean
	require
		pos in 0 .. N - 1 // Valid position
	is
		return this (pos) = 0b1
	end
	routine () (pos: Integer): Integer
	/// Get bit value at position 'pos'
	require
		pos in 0 .. N - 1 /// Valid position	 
	is
		if (data (pos) = 0b0)
			return 0
		else
			retrun 1
		end
	ensure
		return in 0..1 /// Valid Integer from Bit 
	end
	routine toInteger: Integer is
		/// Build Integer from Bit
		result is 0
		factor is 1
		while pos in 0 .. N - 1 loop
			if this (pos) = 1 then
				result := result + factor
				factor := factor * 2
			end
		end
		return result
	end
	routine getLowBits (bitsCount: Integer) : Bit [bitsCount]
	require
		bitsCount in 1 .. N /// Valid Bits Count
	is
		result is Bit [bitsCount] // Hmmm .... Dynamic bit field allocation
		while pos in 0 .. bitsCount - 1 loop
			result (pos) := this (pos)
		end
		return result
	end
	routine toggle (pos: Integer)
	require
		pos in 0 .. N - 1 /// validPos
	is
		this (pos) := if this (pos) = 1 then 0 else 1
	ensure
		( old this )(pos) /= this (pos) /// bit Toggled
	end
	init is
		/// Every BIT field is initialized with 0s
		while pos in 0 .. N - 1 loop
			this (pos) := 0
		end
	end
invariant
	N > 0 /// Number of Bits is greater than zero
end // Bit

abstract unit Any use const Integer, Real, Boolean, Character, String, Bit [Integer] is
	// Adding all constant objects from basic untis allows to use these constants without respective unit name prefix.


	/// Shallow equality tests
    = (that: ? as this ): Boolean is foreign
	final /= (that: ? as this ): Boolean is return not ( this = that) end
    = (that: as this): Boolean is foreign
	final /= (that: as this ): Boolean is return not ( this = that) end
	
	/// Deep equality tests
    == (that: ? as this ): Boolean is foreign
	final /== (that: ? as this ): Boolean is return not ( this == that) end
    == (that: as this): Boolean is foreign
	final /== (that: as this ): Boolean is return not ( this == that) end
	
	/// Assignment definition
	hidden := (that: ? as this ) is foreign
	hidden := (that: as this ) is foreign
	
	/// Utility
	toString: String is foreign
	sizeof: Integer is foreign
	
end // Any

final unit System is
	clone (object: Any): as object is foreign
		/// Shallow version of the object clone operation
	deepClone (object: Any): as object is foreign
		/// Deep version of the object clone operation
end // System

final unit Platform is
	const IntegerBitsCount is 32
	const RealBitsCount is 64
	const CharacterBitsCount is 8
	const BooleanBitsCount is 8
	const PointerBitsCount is 32
	const BitsInByteCount is 8
end // Platform

abstract unit Numeric is
	one: as this is abstract
		/// Definition of One
	zero: as this abstract
		/// Definition of Zero
	+ (other: as this): as this is abstract
		/// Definition of addition
	ensure
		this - other = old this
	end // +
	- (other: as this): as this is abstract
		/// Definition of subtraction 
	ensure
		this + other = old this		
	end // -
	- : as this is abstract
		/// Definition of negation
	ensure
		- old this = this
	end
	/ (other: as this): as this is abstract
		/// Definition of division
	end
	* (other: as this): as this is abstract
		/// Definition of multiplication
	end
	** (other: as this): as this is abstract
		/// Definition of power
	end
invariant
	this = this * one  /// Multiplication constituent
	this = this + zero /// Addition constituent
	this = this ** one  /// Power constituent first
	one = this ** zero  /// Power constituent second
	zero = this - this /// Zero constituent
end // Numeric

abstract unit Comparable is
	< (other: as this): Boolean is abstract
	// It looks we may rely on default equality test from Any ...
	/*override abstract = (other: as this): Boolean
	end*/
	> (other: as this): Boolean => other < this
	<= (other: as this): Boolean => not (other < this)
	>= (other: as this): Boolean => not (this < other)
	max (other: as this): as this => if this > other then this else other
	min (other: as this): as this => if this < other then this else other
invariant
	not (this < this) /// Irreflexive comparison
end // Comparable

abstract unit Enumeration extend Comparable is
	succ: as this is abstract
	/// successor of the current object
	ensure
		return > this
	end
	pred: as this is abstract
	/// predecessor of the current object
	ensure
		return < this
	end
	first: as this is abstract
	/// first elelement of enumeration
	last: as this is abstract
	/// last elelement of enumeration
	count: Integer is abstract
	/// number of elelments in the enumeration
	ord: Integer is abstract
	/// order of the current object in the enumeration, order starts with f_i_r_s_t :-) 
	ensure
		return > 0		
	end
invariant 
	succ > pred
	count >= 0
	ord > pred.ord
	ord < succ.ord
end

val unit Pointer is
	/// Unit Pointer allows to introduce address arithmetics in a safe way :-)
	// WIP!!!
	override sizeof: Integer is
		return Platform.PointerBitsCount / Platform.BitsInByteCount
	end
	hidden data : Bit [Platform.PointerBitsCount]	
end // Pointer

val unit Integer extend Integer [Platform.IntegerBitsCount] is
	init (value: as this) is
		data := value.data
	end
	init is
		/// Default init with zero
		data := Bit [Platform.IntegerBitsCount]
	end
end // Integer

val unit Integer [BitsNumber: Integer] extend Numeric, Enumeration is
	const minInteger is - (2 ** (BitsNumber - 1))
	const maxInteger is 2 ** (BitsNumber - 1) - 1
	const is /* That is ordered set defined as range of all Integer constant values (objects) */
		minInteger .. maxInteger
	end
	override const one is 1
	override const zero is 0
	override sizeof: Integer is
		return BitsNumber / Platform.BitsInByteCount
	end
	succ: as this => this + one
	pred: as this => this - one	
	override const first is minInteger
	override const last is maxInteger
	count: Integer => last - first + 1
	ord: Integer => this - minInteger + 1
	override + (other: as this): as this is
		// Bit-level implementation of Integer addition
		raise "Not implemented!!!"
	end
	+ (other: Real): as this is
		// Bit-level implementation of Integer addition
		raise "Not implemented!!!"
	end
	+ is
		// Unary plus
	end
	override - (other: as this): as this is
		// Bit-level implementation of Integer subtraction
		raise "Not implemented!!!"
	end
	- (other: Real): as this is
		// Bit-level implementation of Integer addition
		raise "Not implemented!!!"
	end
	- is
		// Unary minus
		return 0 - this
	end
	override / (other: as this): as this is
		// Bit-level implementation of Integer division
		raise "Not implemented!!!"
	end
	\ (other: as this): as this is
		// Bit-level implementation of Integer division reminder
		raise "Not implemented!!!"
	end
	/ (other: as this): Real is
		// Bit-level implementation of precise division
		raise "Not implemented!!!"
	end
	/ (other: Real): Real is
		// Bit-level implementation of precise division
		raise "Not implemented!!!"
	end
	override * (other: as this): as this is
		// Bit-level implementation of Integer multiplication
		raise "Not implemented!!!"
	end
	* (other: Real): Real is
		return other * this
	end
	override ** (other: as this): as this is
		// Bit-level implementation of Integer rasing to power
		raise "Not implemented!!!"
	end
	override < (other: as this): Boolean is
		// Bit-level implementation of Integer less
		raise "Not implemented!!!"
	end
	override = (other: as this): Boolean is
		// Bit-level implementation of Inetegr equal
		return data = other.data
	end
	override := (other: as this) is
		init (other)
	end
	:= (other: Real) is
		// Bit-level implementation of Real assignment
		raise "Not implemented!!!"
	end
	:= (other: Character) is
		// Bit-level implementation of Chracter assignment
	raise "Not implemented!!!"
		if data.sizeof = other.data.sizeof then
			data := other.data // Size of data is different!!!
		else
		end
	end
	toCharacter: Character
	require
		this in 0 .. 2 ** Platform.CharacterBitsCount /// containsChacraterCode: 
	is
		return Character.init (data.getLowBits (Platform.CharacterBitsCount))
	end
	init (value: as this) is
		data := value.data
	end
	init is
		/// Default init
		data := Bit [BitsNumber]
	end
	hidden data: Bit [BitsNumber]	
invariant
	BitsNumber > 0 /// Number of bits in Integer must be greater than zero!
end // Integer


val unit Real extend Numeric, Comparable is
	const minReal is -0.0 /// TBD!!!
	const maxReal is +0.0  /// TBD!!
	const is
		minReal .. maxReal
	end
	override const one is 1.0
	override const zero is 0.0
	override sizeof: Integer is
		return Platform.RealBitsCount / Platform.BitsInByteCount
	end
	override + (other: as this): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	+ alias plus(other: Integer): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	+ alias plus is
		// Unary plus
	end
	override - (other: as this): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	- alias minus (other: Integer): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	- alias minus is
		// Unary minus 
		// Bit-level implemenation
		raise "Not implemented!!!"
	end
	override / (other: as this): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	/ (other: Integer): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override * (other: as this): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	* (other: Integer): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override ** (other: as this): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	** (other: Integer): as this is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override < (other: as this): Boolean is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override < (other: Integer): Boolean is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override = (other: as this): Boolean is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override = (other: Integer): Boolean is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override := (other: as this) is
		// Bit-level implementation
		data := other.data
	end
	:= (other: Integer) is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	init (value: as this) is
		data := value.data
	end
	init is
		data := Bit [Platform.RealBitsCount]
	end
	hidden data : Bit [Platform.RealBitsCount]	
end // Real

val unit Character extend Enumeration is
	const minCharacter is 0.ToCharacter
	const maxCharacter is (2 ** Platform.CharacterBitsCount - 1).ToCharacter
	const is // The set of all constant objects of unti type Character
		minCharacter .. maxCharacter
	end	
	succ: as this => (this.toInteger + one).toCharacter
	pred: as this => (this.toInteger - one).ToCharacter
	override const first is minCharacter
	override const last is maxCharacter
	const count is 2 ** Platform.CharacterBitsCount
	ord: Integer => toInteger
	override < (other: as this): Boolean is
		// Bit-level implementation
		raise "Not implemented!!!"
	end
	override sizeof: Integer => Platform.CharacterBitsCount / Platform.BitsInByteCount
	toInteger: Integer is
		return Integer.init (data.ToInteger)
	ensure
		return in return.minInteger .. return.maxInteger
	end
	:= (other: Integer)
	require
		0 <= other and then other <= 2 ** Platform.CharacterBitsCount - 1 /// Invalid character code
	is
		data := other
	end
	init (value: as this) is
		data := value.data
	end
	init is
		data := Bit [Platform.CharacterBitsCount]
	end
	hidden data : Bit [Platform.CharacterBitsCount]	
end // Character

val unit Boolean extend Enumeration is
	const is // That is ordered set of two constant object of type Boolean
		false.init (0), true.init (1)
	end
	override < (other: as this): Boolean => not this => other
	override = (other: as this): Boolean => this.data = other.data
	succ: as this => if this then false else true
	pred: as this => if this then false else true
	override const first is false
	override const last is true
	const count is 2
	ord: Integer => if this then 1 else 0 
	override sizeof: Integer => Platform.BooleanBitsCount / Platform.BitsInByteCount
	& alias and (other: as this): Boolean => if this then if other then true else false else false
		// 00 -> 0
		// 01 -> 0
		// 10 -> 0
		// 11 -> 1
	| alias or (other: as this): Boolean => if this = false then if other then true else false else true	
		// 00 -> 0
		// 01 -> 1
		// 10 -> 1
		// 11 -> 1
	and then (other: as this): Boolean => this and other
	or else (other: as this): Boolean => this or other
	^ alias xor (other: as this): Boolean => if this then if other then false else true else if other then true else false		
		// 00 -> 0
		// 01 -> 1
		// 10 -> 1
		// 11 -> 0
	=> alias implies (other: as this): Boolean => not this or other
	~ alias not : Boolean => if this then false else true
	toInteger: Integer => if this then 1 else 0
	init (value: as this) is
		data := value.data
	end
	init is
		data := 0xb
	end
	hidden init (value: Integer) 
	require	
		value in 0..1 /// Valid Integer value
	is
		data := value
	end
	hidden data: Bit [Platform.BooleanBitsCount]	
invariant
	this and this = this /// idempotence of 'and'
	this or this = this /// idempotence of 'or'
	this and not this = false /// complementation
	this or not this = true /// complementation
end // Boolean

abstract unit AString extend Comparable is
	/// Abstraction of String
	const is
		// "" .. "Character*"
	end
end

unit String [N:Integer] extend AString, Array [Character, N] is
	/// String with fixed length

end

unit String extend AString is
	/// Variable length String
	override < (other: as this): Boolean is
		if this = other then
			return false
		end
		// Check the content
		while pos in 1..count loop
			// TODO WIP
	raise "Not implemented!!!"
		end
		return false
	end
	override = (other: as this): Boolean is
		if this.data = other.data then
			return true
		end
	raise "Not implemented!!!"
		// Check the content to code !!! WIP
		return true
	end
	count: Integer is
		return data.count
	end	
	() (pos: Integer): Character
		/// Get character at position pos
	require 
		1 <= pos and then pos <= count /// Valid element index
	is
		return data (pos)
	end
	() (pos: Integer; value: G) 
		/// Set array element at position pos with value
	require 
		1 <= pos and then pos <= count /// Valid element index 
	is	
		data (pos) := value
	end
	() (from, to: Integer): as this
	// substring
	require	
		from <= to /// Valid charters' indices range
		1 <= from <= count ///
		1 <= to <= count ///
	is
		// WIP to implement
		raise "Not implemented!!!"
	end
	+ (other: as this): as this 
	is
		// WIP to implement
		raise "Not implemented!!!"
	ensure
		return.count = this.count + other.count /// Valid concatenated string length
	end
	init is
		data := Array [Character] (1, 0)
	end
	init (other: as this) is
		data := other.data
	end
	hidden data: Array [Character] // Dynamic empty array of characters
end // String

abstract unit AnArray [G] is
	///   General one dimensional array  
	() (pos: Integer): G 
		/// Get array element at position pos
		require 
			count > 0 /// Array is not empty
			lower <= pos and then pos <= upper /// Valid element index
		is abstract
	end
	() (pos: Integer; value: G) 
		/// Set array element at position pos with value
		require 
			count > 0 /// Array is not empty
			lower <= pos and then pos <= upper /// Valid element index 
		is abstract
	end
	final count: Integer is  
		/// Number of elelments in the array
		return upper – lower + 1 
	end
	lower: Integer is abstract
		/// Lower index value
	upper: Integer is abstract
		/// Upper index value
	() (from, to: Integer): as this  // get subarray
	require
		from <= to /// subarray not empty		
		lower <= from /// from is greater or equal than lower
		to <= upper /// to is less or equal than upper
	is abstract
	ensure
		return.count = to - from + 1 /// resulting subarray has valid numebr of elelments 
	end
	() (start: Integer; subArray: as this)  // put subarray
	require
		lower <= start and then start <= upper /// valid start position of subarray to be put
		start + subArray.count - 1 <= upper /// valid subarray length
	is
		while pos in start .. start + subArray.count - 1 loop
			this (pos) := subArray (subArray.lower + pos - 1)
		end
	end
invariant
	count >= 0 /// Consistent array count must be greater than zero
	lower <= upper - 1 /// Consistent  array range – lower index is not greater than upper when array is not empty. Array is empty when lower = upper - 1
end // AnArray [G]

abstract unit OneDimentionalArray [G->Any init ()] extend AnArray [G] is
	override () (pos: Integer): G is foreign
	override () (pos: Integer; value: G) is foreign
	override lower: Integer is abstract
	
	override upper: Integer is abstract 
	override () (from, to: Integer): as this is abstract
end

unit Array [G->Any init (), N: Integer] extend OneDimentionalArray [G] is
 ///   One dimentional static array .... WIP !!!
/*
	a0 is Array [Integer, 5]
	// That is what we as to have ....
*/
	override const lower: Integer is 1
		
	override upper: Integer is 
		return N 
	end
	override () (from, to: Integer): as this 
	/// get subarray
	is 
		result is Array [G, (from, to)]
		while pos in from .. to loop
			result (pos) := this (pos)
		end
		return result
	end
	init (other: as this) is
		while pos in other.lower .. other.upper loop
			this (lower + pos - 1) := other (pos)
		end
	end
	init is
		element is G.init
		while pos in lower .. upper loop
			this (pos) := element
		end
	end
end // Array [G->Any init (), N: Integer]

unit Array [G->Any init (), N: (Integer,Integer)] extend OneDimentionalArray [G] is
 ///   One dimentional static array .... WIP !!!
/*
	a1 is Array [Real, (10,25)]
	// That is what we as to have ....
*/
	override lower: Integer is
		return N (1)	
	end
	override upper: Integer is
		return N (2)
	end
	override () (from, to: Integer): as this 
	/// get subarray
	is 
		result is Array [G, (from, to)]
		while pos in from .. to loop
			result (pos) := this (pos)
		end
		return result
	end
	init (other: as this) is
		while pos in other.lower .. other.upper loop
			this (lower + pos - 1) := other (pos)
		end
	end
	init is
		element is G.init
		while pos in lower .. upper loop
			this (pos) := element
		end
	end
end // Array [G->Any init (), N: (Integer,Integer)]


unit Array [G -> Any init ()] extend AnArray [G]  is
///   Dynamic array
// We can put info Array only objects which has constructor with empty signature !!! We are always safe - no uninitilized data!!! 
	override () (pos: Integer): G is foreign
	override () (pos: Integer; value: G) is foreign
	override lower: Integer
	override upper: Integer
	override () (from, to: Integer): as this 
	/// get subarray
	is 
		result is Array [G].init(from, to)
		while pos in from .. to loop
			result (pos) := this (pos)
		end
		return result
	end
	init 
		/// It creates an empty array, but lower index is set a 1
	is 
		init (1, 0, G()) 
	end
	init (n: Integer; value: G) 
		/// It creates an array of n elements,  lower index is 1
	require
		n >= 0 
	is 
		init (1, n, value) 
	end
	init (n: Integer) 
	require
		n >= 0 
	is 
		init (1, n, G()) 
	end
	init (l, u: Integer) is
		init (l, u, G())
	end
	init (l, u: Integer; value: G) 
	require
		l <= u - 1 /// Consistent array borders, lower must be not greater than upper if array is not empty and 1 less than upper otherwise
	is 
		lower := l; 
		upper := u; 
		fill (value); 
	end
	hidden fill (value: G) is
		if lower <= upper then
			data := allocateArray (lower, upper, value.sizeof)
			while index in lower .. upper loop  
				this (index) := value
			end
		end
	end
	hidden data: ?Pointer
	hidden foreign allocateArray (l, u: Integer: size: Integer)
	require
		l <= u /// Consistent array borders
	end
end // Array [G->Any init ()]

unit Array [G->Any init (), dimentions: (Integer|Integer .. Integer)] extend Array [G, numberOfElementsInLinearArray ] is
///   Multi dimentional static array .... WIP!!!
/*
	a0 is Array [Integer, (5, 4)]
	a1 is Array [Real, (1..4, 5..7, 3..7)]
	// That is what we as to have ....
*/
	//hidden data: Array [G ......] /// Multi dimetional array is to be stored as one dimentional one!!! 
	hidden numberOfElementsInLinearArray: Integer is
		result is 0
		while dim in dimentions loop
			if dim is 
				Integer:
					result := result + dim
				Integer .. Integer:
					result := result + dim.last - dim.first + 1
			else
				raise "Bad array"
			end
		end
		return result
	end
end // Array [G->Any init (), dimentions: (Integer|Integer .. Integer)]

unit StandardIO is
	// Init procedure is missed or how to set the output is missed !!!!
	put (arguments: ()) is
		while argument in arguments loop
			if argument is
			Integer:
				putInteger (argument)
			Character:
				putCharacter (argument)
			String:
				putString (argument)
			Real:
				putReal (argument)
			Boolean:
				putBoolean (argument)
			else
				putString (argument.ToString)
			end
		end
	end put
	hidden putInteger (anInteger: Integer) is foreign
	hidden putCharacter (aCharacter: Character) is foreign
	hidden putString (aString: String) is foreign
	hidden putReal (aReal: Real) is foreign
	hidden putBoolean (aBoolean: Boolean) is foreign
end StandardIO

abstract unit Routine [Arguments->(), Result] is
// If we rename apply as () - will it work????
	arguments: as Arguments is foreign
	() (args: Arguments) is abstract
	// was apply
		// That is a procedure call
	() (args: Arguments): Result is abstract// was apply
		// That is a function call
end // Routine [Arguments->(), Result]

unit Procedure [Arguments -> ()] extend Routine [Arguments, ()] is
	override () (args: Arguments) is foreign// was apply
		// That is a procedure call
	override hidden () (args: Arguments): Result is end // was apply
		// That is a function call
end // Procedure [Arguments -> ()]

unit Function [Arguments -> (), Result] extend Routine [Arguments, Result] is
	override hidden () (args: Arguments) is end // was apply
		// That is a procedure call
	override () (args: Arguments): Result is foreign // apply
		// That is a function call
end // Function [Arguments -> (), Result]
