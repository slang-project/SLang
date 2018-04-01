
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
