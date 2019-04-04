using System;
using System.IO;

namespace OpenCTM
{
	public class CtmFileReader
	{
		public static readonly int OCTM = getTagInt("OCTM");
		
		private static readonly MeshDecoder[] DECODER = new MeshDecoder[]{
			new RawDecoder(),
			new MG1Decoder(),
			new MG2Decoder()
		};
		
	    private String comment;
	    private readonly CtmInputStream input;
	    private bool decoded;
	
	    public CtmFileReader(Stream source)
	    {
	        input = new CtmInputStream(source);
	    }
	
	    public Mesh decode()
	    {
	        if (decoded) {
	            throw new Exception("Ctm File got already decoded");
	        }
	        decoded = true;
	
	        if (input.readLittleInt() != OCTM) {
	            throw new BadFormatException("The CTM file doesn't start with the OCTM tag!");
	        }
	
	        int formatVersion = input.readLittleInt();
	        int methodTag = input.readLittleInt();
	
	        MeshInfo mi = new MeshInfo(input.readLittleInt(),//vertex count
	                input.readLittleInt(), //triangle count
	                input.readLittleInt(), //uvmap count
	                input.readLittleInt(), //attribute count
	                input.readLittleInt());                  //flags
	
	        comment = input.readString();
	
	        // Uncompress from stream
	        Mesh m = null;
	        foreach (MeshDecoder md in DECODER) {
	            if (md.isFormatSupported(methodTag, formatVersion)) {
	                m = md.decode(mi, input);
	                break;
	            }
	        }
	
	        if (m == null) {
	            throw new IOException("No sutible decoder found for Mesh of compression type: " + unpack(methodTag) + ", version " + formatVersion);
	        }
	
	        // Check mesh integrity
	        m.checkIntegrity();
	
	        return m;
	    }
	
	    public static String unpack(int tag)
	    {
	        byte[] chars = new byte[4];
	        chars[0] = (byte) (tag & 0xff);
	        chars[1] = (byte) ((tag >> 8) & 0xff);
	        chars[2] = (byte) ((tag >> 16) & 0xff);
	        chars[3] = (byte) ((tag >> 24) & 0xff);
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
	        return enc.GetString(chars);
	    }
	
	    /**
	     * before calling this method the first time, the decode method has to be
	     * called.
	     * <p/>
	     * @throws RuntimeExceptio- if the file wasn't decoded before.
	     */
	    public String getFileComment()
	    {
	        if (!decoded) {
	            throw new Exception("The CTM file is not decoded yet.");
	        }
	        return comment;
	    }
	
	    public static int getTagInt(String tag)
	    {
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
	        byte[] chars = enc.GetBytes(tag);
	        if(chars.Length != 4)
				throw new Exception("A tag has to be constructed out of 4 characters!");
	        return chars[0] | (chars[1] << 8) | (chars[2] << 16) | (chars[3] << 24);
	    }
	}
}

