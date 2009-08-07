/*
 * User: scorder
 * Date: 7/7/2009
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using MongoDB.Driver;

namespace MongoDB.Driver.Bson
{
	/// <summary>
	/// Description of BSONDocument.
	/// </summary>
	public class BsonDocument : System.Collections.DictionaryBase, BsonType
	{
		private List<String> orderedKeys = new List<String>();
		
		public BsonDocument(){
		}
		public BsonElement this[ String key ]  {
			get{
				return (BsonElement)Dictionary[key];
			}
			set{
				if(orderedKeys.Contains(key) == false){
					orderedKeys.Add(key);
				}
				Dictionary[key] = value;
			}
		}
		
		public ICollection Keys  {
			get  {
				return(orderedKeys);
			}
		}
		
		public ICollection Values  {
			get  {
				return( Dictionary.Values );
			}
		}

		public void Add( String key, BsonElement value )  {
			if(orderedKeys.Contains(key)) throw new ArgumentException("Key already exists");
			this[key] = value;
		}
		public void Add(BsonElement value){
			this.Add(value.Name, value);
		}
		public void Add(String key, BsonType val){
			this.Add(new BsonElement(key, val));
		}
		
		public BsonDocument Append(String key, BsonElement value){
			this.Add(key,value);
			return this;
		}
		
		public BsonDocument Update(BsonDocument from){
			if(from == null) return this;
			foreach(String key in from.Keys){
				this[key] = from[key];
			}
			return this;
		}
		
		public bool Contains( String key )  {
			return( orderedKeys.Contains( key ) );
		}
		
		public void Remove( String key )  {
			Dictionary.Remove( key );
			orderedKeys.Remove(key);
		}
		
		public byte TypeNum {
			get {return (byte)BsonDataType.Obj;}
		}
		
		public int Size {
			get {
				int size = 4;
				foreach(String key in this.Keys){
					BsonElement be = this[key];
					size += be.Size;
				}
				size += 1; //Object terminator
				return size;
			}
		}		
		
		public void Write(BsonWriter writer){
			writer.Write(this.Size);
			foreach(String key in this.Keys){
				BsonElement be = this[key];
				be.Write(writer);
			}
			writer.Write((byte)0);
		}
		
//		public void Read(BsonReader reader){
//			int size = reader.ReadInt32();
//			foreach(String key in this.Keys){
//				BsonElement be = this[key];
//				be.Read(reader);
//			}
//			byte eoo = reader.ReadByte();
//			size++;
//			if(size != this.Size) throw new System.IO.InvalidDataException("Document is not the same size as the stream reported");
//		}
		
		public int Read(BsonReader reader){
			int size = reader.ReadInt32();
			int bytesRead = 4;
//			try{
				while(bytesRead + 1 < size){
					BsonElement be = new BsonElement();
					bytesRead += be.Read(reader);
					this.Add(be);
				}
				byte eoo = reader.ReadByte();
				bytesRead++;
				if(eoo != (byte)0) throw new System.IO.InvalidDataException("Document not null terminated");			
				if(size != bytesRead) {
					throw new System.IO.InvalidDataException(string.Format("Should have read {0} bytes from stream but only read {1}]", size, bytesRead));
				}
//			}catch(ArgumentOutOfRangeException aor){
//				throw new System.IO.InvalidDataException("Document out of sync with stream bytesRead: " + bytesRead + " size: " + size + " Doc: " + this.ToString());
//			}
			return bytesRead;
		}
		
		public object ToNative(){
			Document doc = new Document();
			foreach(String key in this.Keys){
				BsonElement be = this[key];
				doc[key] = be.Val.ToNative();
			}
			return doc;
		}	
		
		public override string ToString ()
		{
			return string.Format("[BsonDocument: TypeNum={0}, Size={1}]", TypeNum, Size);
		}
	}
}
