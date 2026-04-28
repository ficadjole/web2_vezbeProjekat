export default function Counter({ count, onCountChange }) {
  return (
    <>
      <div className="w-44">
        <p className="text-center bg-gray-200 text-black">{count}</p>
        <div className="grid grid-cols-3 gap-3 mt-3">
          <div>
            {" "}
            <button
              type="button"
              className="bg-green-200 w-10 rounded-md"
              onClick={() => onCountChange(count + 1)}
            >
              +
            </button>
          </div>
          <div>
            {" "}
            <button
              type="button"
              className="bg-red-800 w-10 rounded-md"
              onClick={() => onCountChange(count - 1)}
            >
              -
            </button>
          </div>
          <div>
            <button
              type="button"
              className="bg-green-200 w-10 rounded-md"
              onClick={() => onCountChange(0)}
            >
              reset
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
