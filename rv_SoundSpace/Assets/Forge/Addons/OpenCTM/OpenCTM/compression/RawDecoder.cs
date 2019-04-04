using System;

namespace OpenCTM
{
	public class RawDecoder : MeshDecoder
	{
		public static readonly int RAW_TAG = CtmFileReader.getTagInt("RAW\0");
	    public const int FORMAT_VERSION = 5;
	
	    public override Mesh decode(MeshInfo minfo, CtmInputStream input)
	    {
	        int vc = minfo.getVertexCount();
	
	        AttributeData[] tex = new AttributeData[minfo.getUvMapCount()];
	        AttributeData[] att = new AttributeData[minfo.getAttrCount()];
	
	        checkTag(input.readLittleInt(), INDX);
	        int[] indices = readIntArray(input, minfo.getTriangleCount(), 3, false);
	
	        checkTag(input.readLittleInt(), VERT);
	        float[] vertices = readFloatArray(input, vc * Mesh.CTM_POSITION_ELEMENT_COUNT, 1);
	
	        float[] normals = null;
	        if (minfo.hasNormals()) {
	            checkTag(input.readLittleInt(), NORM);
	            normals = readFloatArray(input, vc, Mesh.CTM_NORMAL_ELEMENT_COUNT);
	        }
	
	        for (int i = 0; i < tex.Length; ++i) {
	            checkTag(input.readLittleInt(), TEXC);
	            tex[i] = readUVData(vc, input);
	        }
	
	        for (int i = 0; i < att.Length; ++i) {
	            checkTag(input.readLittleInt(), ATTR);
	            att[i] = readAttrData(vc, input);
	        }
	
	        return new Mesh(vertices, normals, indices, tex, att);
	    }
	
	    protected void checkTag(int readTag, int expectedTag)
	    {
	        if (readTag != expectedTag) {
	            throw new BadFormatException("Instead of the expected data tag(\"" + CtmFileReader.unpack(expectedTag)
	                    + "\") the tag(\"" + CtmFileReader.unpack(readTag) + "\") was read!");
	        }
	    }
	
	    protected virtual int[] readIntArray(CtmInputStream input, int count, int size, bool signed)
	    {
	        int[] array = new int[count * size];
	        for (int i = 0; i < array.Length; i++) {
	            array[i] = input.readLittleInt();
	        }
	        return array;
	    }
	
	    protected virtual float[] readFloatArray(CtmInputStream input, int count, int size)
	    {
	        float[] array = new float[count * size];
	        for (int i = 0; i < array.Length; i++) {
	            array[i] = input.readLittleFloat();
	        }
	        return array;
	    }
	
	    private AttributeData readUVData(int vertCount, CtmInputStream input)
	    {
	        String name = input.readString();
	        String matname = input.readString();
	        float[] data = readFloatArray(input, vertCount, Mesh.CTM_UV_ELEMENT_COUNT);
	
	        return new AttributeData(name, matname, AttributeData.STANDARD_UV_PRECISION, data);
	    }
	
	    private AttributeData readAttrData(int vertCount, CtmInputStream input)
	    {
	        String name = input.readString();
	        float[] data = readFloatArray(input, vertCount, Mesh.CTM_ATTR_ELEMENT_COUNT);
	
	        return new AttributeData(name, null, AttributeData.STANDARD_PRECISION, data);
	    }
	
	    public override bool isFormatSupported(int tag, int version)
	    {
	        return tag == RAW_TAG && version == FORMAT_VERSION;
	    }
	}
}

