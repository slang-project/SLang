unit Integer is end
unit Array[G,(Integer,Integer)] is end

unit X[from: Integer, to: Integer] is
    override routine () (from, to: Integer): as this 
	/// get subarray
	is 
		result is Array [G, (from, to)]
    end
end