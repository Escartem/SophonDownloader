// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: SophonManifestProto.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021, 8981
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace Hi3Helper.Sophon.Protos {

  /// <summary>Holder for reflection information generated from SophonManifestProto.proto</summary>
  public static partial class SophonManifestProtoReflection {

    #region Descriptor
    /// <summary>File descriptor for SophonManifestProto.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static SophonManifestProtoReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "ChlTb3Bob25NYW5pZmVzdFByb3RvLnByb3RvEhdIaTNIZWxwZXIuU29waG9u",
            "LlByb3RvcyJbChNTb3Bob25NYW5pZmVzdFByb3RvEkQKBkFzc2V0cxgBIAMo",
            "CzI0LkhpM0hlbHBlci5Tb3Bob24uUHJvdG9zLlNvcGhvbk1hbmlmZXN0QXNz",
            "ZXRQcm9wZXJ0eSK0AQobU29waG9uTWFuaWZlc3RBc3NldFByb3BlcnR5EhEK",
            "CUFzc2V0TmFtZRgBIAEoCRJGCgtBc3NldENodW5rcxgCIAMoCzIxLkhpM0hl",
            "bHBlci5Tb3Bob24uUHJvdG9zLlNvcGhvbk1hbmlmZXN0QXNzZXRDaHVuaxIR",
            "CglBc3NldFR5cGUYAyABKAUSEQoJQXNzZXRTaXplGAQgASgDEhQKDEFzc2V0",
            "SGFzaE1kNRgFIAEoCSKcAQoYU29waG9uTWFuaWZlc3RBc3NldENodW5rEhEK",
            "CUNodW5rTmFtZRgBIAEoCRIgChhDaHVua0RlY29tcHJlc3NlZEhhc2hNZDUY",
            "AiABKAkSGQoRQ2h1bmtPbkZpbGVPZmZzZXQYAyABKAMSEQoJQ2h1bmtTaXpl",
            "GAQgASgDEh0KFUNodW5rU2l6ZURlY29tcHJlc3NlZBgFIAEoA2IGcHJvdG8z"));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(null, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::Hi3Helper.Sophon.Protos.SophonManifestProto), global::Hi3Helper.Sophon.Protos.SophonManifestProto.Parser, new[]{ "Assets" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty), global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty.Parser, new[]{ "AssetName", "AssetChunks", "AssetType", "AssetSize", "AssetHashMd5" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk), global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk.Parser, new[]{ "ChunkName", "ChunkDecompressedHashMd5", "ChunkOnFileOffset", "ChunkSize", "ChunkSizeDecompressed" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Messages
  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class SophonManifestProto : pb::IMessage<SophonManifestProto>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SophonManifestProto> _parser = new pb::MessageParser<SophonManifestProto>(() => new SophonManifestProto());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SophonManifestProto> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Hi3Helper.Sophon.Protos.SophonManifestProtoReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestProto() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestProto(SophonManifestProto other) : this() {
      assets_ = other.assets_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestProto Clone() {
      return new SophonManifestProto(this);
    }

    /// <summary>Field number for the "Assets" field.</summary>
    public const int AssetsFieldNumber = 1;
    private static readonly pb::FieldCodec<global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty> _repeated_assets_codec
        = pb::FieldCodec.ForMessage(10, global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty.Parser);
    private readonly pbc::RepeatedField<global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty> assets_ = new pbc::RepeatedField<global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Hi3Helper.Sophon.Protos.SophonManifestAssetProperty> Assets {
      get { return assets_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SophonManifestProto);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SophonManifestProto other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if(!assets_.Equals(other.assets_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      hash ^= assets_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      assets_.WriteTo(output, _repeated_assets_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      assets_.WriteTo(ref output, _repeated_assets_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      size += assets_.CalculateSize(_repeated_assets_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SophonManifestProto other) {
      if (other == null) {
        return;
      }
      assets_.Add(other.assets_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            assets_.AddEntriesFrom(input, _repeated_assets_codec);
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            assets_.AddEntriesFrom(ref input, _repeated_assets_codec);
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class SophonManifestAssetProperty : pb::IMessage<SophonManifestAssetProperty>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SophonManifestAssetProperty> _parser = new pb::MessageParser<SophonManifestAssetProperty>(() => new SophonManifestAssetProperty());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SophonManifestAssetProperty> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Hi3Helper.Sophon.Protos.SophonManifestProtoReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestAssetProperty() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestAssetProperty(SophonManifestAssetProperty other) : this() {
      assetName_ = other.assetName_;
      assetChunks_ = other.assetChunks_.Clone();
      assetType_ = other.assetType_;
      assetSize_ = other.assetSize_;
      assetHashMd5_ = other.assetHashMd5_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestAssetProperty Clone() {
      return new SophonManifestAssetProperty(this);
    }

    /// <summary>Field number for the "AssetName" field.</summary>
    public const int AssetNameFieldNumber = 1;
    private string assetName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string AssetName {
      get { return assetName_; }
      set {
        assetName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "AssetChunks" field.</summary>
    public const int AssetChunksFieldNumber = 2;
    private static readonly pb::FieldCodec<global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk> _repeated_assetChunks_codec
        = pb::FieldCodec.ForMessage(18, global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk.Parser);
    private readonly pbc::RepeatedField<global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk> assetChunks_ = new pbc::RepeatedField<global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk>();
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public pbc::RepeatedField<global::Hi3Helper.Sophon.Protos.SophonManifestAssetChunk> AssetChunks {
      get { return assetChunks_; }
    }

    /// <summary>Field number for the "AssetType" field.</summary>
    public const int AssetTypeFieldNumber = 3;
    private int assetType_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int AssetType {
      get { return assetType_; }
      set {
        assetType_ = value;
      }
    }

    /// <summary>Field number for the "AssetSize" field.</summary>
    public const int AssetSizeFieldNumber = 4;
    private long assetSize_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long AssetSize {
      get { return assetSize_; }
      set {
        assetSize_ = value;
      }
    }

    /// <summary>Field number for the "AssetHashMd5" field.</summary>
    public const int AssetHashMd5FieldNumber = 5;
    private string assetHashMd5_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string AssetHashMd5 {
      get { return assetHashMd5_; }
      set {
        assetHashMd5_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SophonManifestAssetProperty);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SophonManifestAssetProperty other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (AssetName != other.AssetName) return false;
      if(!assetChunks_.Equals(other.assetChunks_)) return false;
      if (AssetType != other.AssetType) return false;
      if (AssetSize != other.AssetSize) return false;
      if (AssetHashMd5 != other.AssetHashMd5) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (AssetName.Length != 0) hash ^= AssetName.GetHashCode();
      hash ^= assetChunks_.GetHashCode();
      if (AssetType != 0) hash ^= AssetType.GetHashCode();
      if (AssetSize != 0L) hash ^= AssetSize.GetHashCode();
      if (AssetHashMd5.Length != 0) hash ^= AssetHashMd5.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (AssetName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(AssetName);
      }
      assetChunks_.WriteTo(output, _repeated_assetChunks_codec);
      if (AssetType != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(AssetType);
      }
      if (AssetSize != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(AssetSize);
      }
      if (AssetHashMd5.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(AssetHashMd5);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (AssetName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(AssetName);
      }
      assetChunks_.WriteTo(ref output, _repeated_assetChunks_codec);
      if (AssetType != 0) {
        output.WriteRawTag(24);
        output.WriteInt32(AssetType);
      }
      if (AssetSize != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(AssetSize);
      }
      if (AssetHashMd5.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(AssetHashMd5);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (AssetName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AssetName);
      }
      size += assetChunks_.CalculateSize(_repeated_assetChunks_codec);
      if (AssetType != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(AssetType);
      }
      if (AssetSize != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(AssetSize);
      }
      if (AssetHashMd5.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(AssetHashMd5);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SophonManifestAssetProperty other) {
      if (other == null) {
        return;
      }
      if (other.AssetName.Length != 0) {
        AssetName = other.AssetName;
      }
      assetChunks_.Add(other.assetChunks_);
      if (other.AssetType != 0) {
        AssetType = other.AssetType;
      }
      if (other.AssetSize != 0L) {
        AssetSize = other.AssetSize;
      }
      if (other.AssetHashMd5.Length != 0) {
        AssetHashMd5 = other.AssetHashMd5;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            AssetName = input.ReadString();
            break;
          }
          case 18: {
            assetChunks_.AddEntriesFrom(input, _repeated_assetChunks_codec);
            break;
          }
          case 24: {
            AssetType = input.ReadInt32();
            break;
          }
          case 32: {
            AssetSize = input.ReadInt64();
            break;
          }
          case 42: {
            AssetHashMd5 = input.ReadString();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            AssetName = input.ReadString();
            break;
          }
          case 18: {
            assetChunks_.AddEntriesFrom(ref input, _repeated_assetChunks_codec);
            break;
          }
          case 24: {
            AssetType = input.ReadInt32();
            break;
          }
          case 32: {
            AssetSize = input.ReadInt64();
            break;
          }
          case 42: {
            AssetHashMd5 = input.ReadString();
            break;
          }
        }
      }
    }
    #endif

  }

  [global::System.Diagnostics.DebuggerDisplayAttribute("{ToString(),nq}")]
  public sealed partial class SophonManifestAssetChunk : pb::IMessage<SophonManifestAssetChunk>
  #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      , pb::IBufferMessage
  #endif
  {
    private static readonly pb::MessageParser<SophonManifestAssetChunk> _parser = new pb::MessageParser<SophonManifestAssetChunk>(() => new SophonManifestAssetChunk());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pb::MessageParser<SophonManifestAssetChunk> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::Hi3Helper.Sophon.Protos.SophonManifestProtoReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestAssetChunk() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestAssetChunk(SophonManifestAssetChunk other) : this() {
      chunkName_ = other.chunkName_;
      chunkDecompressedHashMd5_ = other.chunkDecompressedHashMd5_;
      chunkOnFileOffset_ = other.chunkOnFileOffset_;
      chunkSize_ = other.chunkSize_;
      chunkSizeDecompressed_ = other.chunkSizeDecompressed_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public SophonManifestAssetChunk Clone() {
      return new SophonManifestAssetChunk(this);
    }

    /// <summary>Field number for the "ChunkName" field.</summary>
    public const int ChunkNameFieldNumber = 1;
    private string chunkName_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ChunkName {
      get { return chunkName_; }
      set {
        chunkName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ChunkDecompressedHashMd5" field.</summary>
    public const int ChunkDecompressedHashMd5FieldNumber = 2;
    private string chunkDecompressedHashMd5_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public string ChunkDecompressedHashMd5 {
      get { return chunkDecompressedHashMd5_; }
      set {
        chunkDecompressedHashMd5_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ChunkOnFileOffset" field.</summary>
    public const int ChunkOnFileOffsetFieldNumber = 3;
    private long chunkOnFileOffset_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long ChunkOnFileOffset {
      get { return chunkOnFileOffset_; }
      set {
        chunkOnFileOffset_ = value;
      }
    }

    /// <summary>Field number for the "ChunkSize" field.</summary>
    public const int ChunkSizeFieldNumber = 4;
    private long chunkSize_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long ChunkSize {
      get { return chunkSize_; }
      set {
        chunkSize_ = value;
      }
    }

    /// <summary>Field number for the "ChunkSizeDecompressed" field.</summary>
    public const int ChunkSizeDecompressedFieldNumber = 5;
    private long chunkSizeDecompressed_;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public long ChunkSizeDecompressed {
      get { return chunkSizeDecompressed_; }
      set {
        chunkSizeDecompressed_ = value;
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override bool Equals(object other) {
      return Equals(other as SophonManifestAssetChunk);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public bool Equals(SophonManifestAssetChunk other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (ChunkName != other.ChunkName) return false;
      if (ChunkDecompressedHashMd5 != other.ChunkDecompressedHashMd5) return false;
      if (ChunkOnFileOffset != other.ChunkOnFileOffset) return false;
      if (ChunkSize != other.ChunkSize) return false;
      if (ChunkSizeDecompressed != other.ChunkSizeDecompressed) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override int GetHashCode() {
      int hash = 1;
      if (ChunkName.Length != 0) hash ^= ChunkName.GetHashCode();
      if (ChunkDecompressedHashMd5.Length != 0) hash ^= ChunkDecompressedHashMd5.GetHashCode();
      if (ChunkOnFileOffset != 0L) hash ^= ChunkOnFileOffset.GetHashCode();
      if (ChunkSize != 0L) hash ^= ChunkSize.GetHashCode();
      if (ChunkSizeDecompressed != 0L) hash ^= ChunkSizeDecompressed.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void WriteTo(pb::CodedOutputStream output) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      output.WriteRawMessage(this);
    #else
      if (ChunkName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ChunkName);
      }
      if (ChunkDecompressedHashMd5.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ChunkDecompressedHashMd5);
      }
      if (ChunkOnFileOffset != 0L) {
        output.WriteRawTag(24);
        output.WriteInt64(ChunkOnFileOffset);
      }
      if (ChunkSize != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(ChunkSize);
      }
      if (ChunkSizeDecompressed != 0L) {
        output.WriteRawTag(40);
        output.WriteInt64(ChunkSizeDecompressed);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
      if (ChunkName.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(ChunkName);
      }
      if (ChunkDecompressedHashMd5.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(ChunkDecompressedHashMd5);
      }
      if (ChunkOnFileOffset != 0L) {
        output.WriteRawTag(24);
        output.WriteInt64(ChunkOnFileOffset);
      }
      if (ChunkSize != 0L) {
        output.WriteRawTag(32);
        output.WriteInt64(ChunkSize);
      }
      if (ChunkSizeDecompressed != 0L) {
        output.WriteRawTag(40);
        output.WriteInt64(ChunkSizeDecompressed);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(ref output);
      }
    }
    #endif

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public int CalculateSize() {
      int size = 0;
      if (ChunkName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ChunkName);
      }
      if (ChunkDecompressedHashMd5.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ChunkDecompressedHashMd5);
      }
      if (ChunkOnFileOffset != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ChunkOnFileOffset);
      }
      if (ChunkSize != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ChunkSize);
      }
      if (ChunkSizeDecompressed != 0L) {
        size += 1 + pb::CodedOutputStream.ComputeInt64Size(ChunkSizeDecompressed);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(SophonManifestAssetChunk other) {
      if (other == null) {
        return;
      }
      if (other.ChunkName.Length != 0) {
        ChunkName = other.ChunkName;
      }
      if (other.ChunkDecompressedHashMd5.Length != 0) {
        ChunkDecompressedHashMd5 = other.ChunkDecompressedHashMd5;
      }
      if (other.ChunkOnFileOffset != 0L) {
        ChunkOnFileOffset = other.ChunkOnFileOffset;
      }
      if (other.ChunkSize != 0L) {
        ChunkSize = other.ChunkSize;
      }
      if (other.ChunkSizeDecompressed != 0L) {
        ChunkSizeDecompressed = other.ChunkSizeDecompressed;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    public void MergeFrom(pb::CodedInputStream input) {
    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
      input.ReadRawMessage(this);
    #else
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            ChunkName = input.ReadString();
            break;
          }
          case 18: {
            ChunkDecompressedHashMd5 = input.ReadString();
            break;
          }
          case 24: {
            ChunkOnFileOffset = input.ReadInt64();
            break;
          }
          case 32: {
            ChunkSize = input.ReadInt64();
            break;
          }
          case 40: {
            ChunkSizeDecompressed = input.ReadInt64();
            break;
          }
        }
      }
    #endif
    }

    #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
    void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
      if ((tag & 7) == 4) {
        // Abort on any end group tag.
        return;
      }
      switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
            break;
          case 10: {
            ChunkName = input.ReadString();
            break;
          }
          case 18: {
            ChunkDecompressedHashMd5 = input.ReadString();
            break;
          }
          case 24: {
            ChunkOnFileOffset = input.ReadInt64();
            break;
          }
          case 32: {
            ChunkSize = input.ReadInt64();
            break;
          }
          case 40: {
            ChunkSizeDecompressed = input.ReadInt64();
            break;
          }
        }
      }
    }
    #endif

  }

  #endregion

}

#endregion Designer generated code
