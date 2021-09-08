using System.Threading.Tasks;

namespace System.IO
{
    // http://blogs.msdn.com/b/kwill/archive/2013/03/06/asynchronous-parallel-block-blob-transfers-with-progress-change-notification-2-0.aspx
    public class ProgressStream
        :
        Stream
    {
        #region Fields

        Stream _parentStream;
        IProgress<int> _progress;
        long _totalLength;

        #endregion

        #region Constructor

        public ProgressStream(Stream parentStream, IProgress<int> progress)
        {
            _parentStream = parentStream;
            _totalLength = parentStream.Length;
            _progress = progress;
        }

        #endregion

        #region Properties

        #region CanRead

        public override bool CanRead
        {
            get { return _parentStream.CanRead; }
        }

        #endregion

        #region CanSeek

        public override bool CanSeek
        {
            get { return _parentStream.CanSeek; }
        }

        #endregion

        #region CanWrite

        public override bool CanWrite
        {
            get { return _parentStream.CanWrite; }
        }

        #endregion

        #region Length

        public override long Length
        {
            get { return _parentStream.Length; }
        }

        #endregion

        #region Position

        public override long Position
        {
            get { return _parentStream.Position; }
            set { _parentStream.Position = value; }
        }

        #endregion

        #region ParentStream

        public Stream ParentStream => _parentStream;

        #endregion

        #endregion

        #region Methods

        #region Close

        public override void Close()
        {
            _parentStream.Close();
        }

        #endregion

        #region Flush

        public override void Flush()
        {
            _parentStream.Flush();
        }

        #endregion

        #region Read

        public override int Read(byte[] buffer, int offset, int count)
        {
            int result = _parentStream.Read(buffer, offset, count);
            _progress.ReportSafe(_parentStream.Position, _totalLength);
            return result;
        }

        #endregion

        #region Seek

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _parentStream.Seek(offset, origin);
        }

        #endregion

        #region SetLength

        public override void SetLength(long value)
        {
            _totalLength = value;
        }

        #endregion

        #region Write

        public override void Write(byte[] buffer, int offset, int count)
        {
            _parentStream.Write(buffer, offset, count);
            _progress.ReportSafe(_parentStream.Position, _totalLength);
        }

        #endregion

        #endregion
    }
}
