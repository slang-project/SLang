val unit Bit [N: Counter] is
	hidden data: val Array [0..N-1, Bit] // Bit field ...
	:= (other: Integer) is
	/// Set N low bits from Integer "other" into this
		//if the number of bits in other is less than N we just set leftmost significant bits to 0b0
		//if the number of bits in other is more than N we just set what fits
		this := other.data
	end
	:= (other: as this) is
		least_N is Counter
		if N > other.N 
			least_N := other.N
		else
			least_N := N

		//filling rightmost bits with valuable bits
		while pos in 0 .. least_N-1 
		loop
			this(pos) := other(pos)
		end

		//filling the rest of the bits with 0b0
		while pos in least_N .. N-1 
		loop
			this(pos) := 0b0
		end
	end
	override sizeof: Counter is
		// return sizeof in overall amount of bytes то есть количетсво целых байтов!!!! N is a number of bits !!!
		return N / Platform.BitsInByteCount + if N / Platform.BitsInByteCount = 0 then 0 else 1
	end
	& alias and (other: as this): as this is
		result is this
		while pos in 0 .. N - 1 
		loop
			result (pos) := if this(pos) = 0 or else other(pos) = 0 then 0 else 1
		end
		return result
	end // and
	| alias or (other: as this): as this is
		result is this
		while pos in 0 .. N - 1 
		loop
			result (pos) := if this(pos) = 1 or else other (pos) = 1 then 1 else 0
		end
		return result
	end // or
	^ alias xor (other: as this): as this is
		result is this
		while pos in 0 .. N - 1 
		loop
			result (pos) := if this (pos) = other(pos) then 0 else 1
		end
		return result
	end
	~ alias not : as this is
		result is this
		while pos in 0 .. N - 1 
		loop
			result.toggle (pos)
		end
		return result
	end // not
	= (other: as this): Boolean is
		while pos in 0 .. N - 1 
		loop
			if this (pos) /= other(pos)
			then
				return false
			end
		end
	end
	>> (bits: Counter) : as this
	require 
		bits in 0 .. N
	is
		result is Bit[N]
		if bits = N
		then
			while pos in 0 .. N-1
			loop
				result(pos) := this(N-1)
			end
			return result
		else
			while pos in 0 .. N-1-bits
			loop
				result(pos) := this(pos+bits)
			end
			while pos in N-bits .. N-1
			loop
				result(pos) := this(N-1)
			end
			retun result
		end
	end
	<< (bits: Counter) : as this
	require 
		bits in 0 .. N
	is
		if bits = 0
		then
			return this
		end
		result is Bit[N]
		if bits = N
		then
			return result
		else
			while pos in N-1 .. bits
			loop
				result(pos) := this(pos-1)
			end
			while pos in bits-1 .. 0
			loop
				result(pos) := this(N-1)
			end
			retun result
		end
	end
	() (pos: Counter, value: Boolean)
	require
		pos in 0 .. N - 1 /// Valid bit position
	is	
		this (pos) := if value then 1 else 0
	end // ()
	() (pos: Counter; value: Integer)
	/// Set bit at position 'pos' to value 'value'
	require
		pos in 0 .. N - 1 /// Valid bit position
		value in 0..1 /// Valid value
	is
		if value = Integer.zero
		then
			data (pos) := 0b0
		else
			data (pos) := 0b1
		end
	end // ()
	() (pos: Counter): Boolean
	require
		pos in 0 .. N - 1 // Valid position
	is
		return this (pos) = 0b1
	end // ()
	() (pos: Counter): Integer
	/// Get bit value at position 'pos'
	require
		pos in 0 .. N - 1 /// Valid position	 
	is
		return data (pos)
	ensure
		return in 0..1 /// Valid Integer from Bit 
	end // ()
	toInteger: Integer is
		/// Build Integer from Bit
		result is Integer.zero
		factor is Integer.one
		two is Integer.one + Integer.one
		while pos in 0 .. N - 1 
		loop
			if this (pos) = 1 
			then
				result := result + factor
				factor := factor * two
			end
		end
		return result
	end // ToInteger
	getLowBits (bitsCount: Counter) : Bit [bitsCount]
	require
		bitsCount in 1 .. N /// Valid Bits Count
	is
		result is Bit [bitsCount] // Hmmm .... Dynamic bit field allocation
		while pos in 0 .. bitsCount - 1 
		loop
			result (pos) := this (pos)
		end
		return result
	end // getLowBits
	toggle is
		while pos in 0 .. N-1
		loop
			toggle(pos)
		end
	end
	toggle (pos: Counter)
	require
		pos in 0 .. N - 1 /// validPos
	is
		this (pos) := if this (pos) = 0b0 then 0b0 else 0b1
	ensure
		( old this )(pos) /= this (pos) /// bit Toggled
	end // toggle
	concat(others: Array[as this]) as this => if others.size = 1 then concat(others(0)) else others(0).concat(others(1,others.size))
	concat(other: as this): as this is
		result is Bit [N+other.N]
		while pos in 0 .. N-1
		loop
			result(pos) := this(pos)
		end
		while pos in 0 .. other.N
		loop
			result(pos+N) := other(pos)
		end
	end
	init is
		/// Every BIT field is initialised with 0s
		while pos in 0 .. N - 1 loop
			this (pos) := 0b0
		end
	end // init
invariant
	N > 0 /// Number of Bits is greater than zero
end